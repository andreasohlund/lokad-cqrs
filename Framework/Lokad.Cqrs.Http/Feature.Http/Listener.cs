#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Lokad.Cqrs.Feature.Http.Events;
using Lokad.Cqrs.Feature.Http.Handlers;

namespace Lokad.Cqrs.Feature.Http
{
    public sealed class Listener : IEngineProcess
    {
        HttpListener _listener = new HttpListener();

        readonly IHttpEnvironment _environment;
        readonly ISystemObserver _observer;
        IEnumerable<AbstractHttpRequestHandler> _handlers;
        readonly SemaphoreSlim _requestSemaphore = new SemaphoreSlim(128);

        public Listener(IHttpEnvironment environment, ISystemObserver observer,
            IEnumerable<AbstractHttpRequestHandler> handlers)
        {
            _environment = environment;
            _observer = observer;
            _handlers = handlers;
        }

        public void Dispose() {}

        public void Initialize()
        {
            _handlers = _handlers.ToArray();
        }

        void GetContext(IAsyncResult ar, CancellationToken token)
        {
            IHttpContext context;
            try
            {
                var listenerContext = _listener.EndGetContext(ar);
                context = new HttpListenerContextAdapter(_environment, listenerContext);

                if (!token.IsCancellationRequested)
                {
                    // setup the next request
                    _listener.BeginGetContext(_ => GetContext(_, token), null);
                }
            }
            catch (InvalidOperationException)
            {
                // stopping probably
                return;
            }
            catch (HttpListenerException)
            {
                // stopping probably
                return;
            }

            if (false == _requestSemaphore.Wait(TimeSpan.FromSeconds(3)))
            {
                HandleTooBusyError(context);
                return;
            }
            try
            {
                ProcessRequestInner(context);
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        int _requestNumber;

        public void ProcessRequestInner(IHttpContext context)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                DispatchRequestToHandlers(context);
            }
            catch (Exception e)
            {
                HandleException(context, e);

                //logger.Warn("Error on request", e);
            }
            finally
            {
                context.TryCloseResponse();

                var curReq = Interlocked.Increment(ref _requestNumber);
                Trace.WriteLine(string.Format("Request #{0,4:#,0}: {1,-7} - {2,5:#,0} ms - {3} - {4}",
                    curReq, context.Request.HttpMethod, sw.ElapsedMilliseconds, context.Response.StatusCode,
                    context.Request.Url.PathAndQuery));
            }
        }

        void DispatchRequestToHandlers(IHttpContext context)
        {
            foreach (var requestResponder in _handlers)
            {
                if (requestResponder.WillHandle(context))
                {
                    requestResponder.Handle(context);
                    return;
                }
            }
            context.SetStatusTo(HttpStatusCode.BadRequest);


            if (context.Request.HttpMethod == "HEAD")
                return;
            context.WriteString(
                @"
<html>
    <body>
        <h1>Bad Request</h1>
        <p>Mea culpa, Lokad.Cqrs.Http server does not know, how to handle this request.</p>
    </body>
</html>
");
        }

        void HandleException(IHttpContext ctx, Exception e)
        {
            try
            {
                // handle specific exceptions

                Trace.WriteLine("Exception");
                Trace.WriteLine(e);
            }
            catch (Exception)
            {
                Trace.WriteLine("Failed to handle error");
                Trace.WriteLine(e);
            }
        }

        static void SerializeError(IHttpContext ctx, object error)
        {
            ctx.WriteString(error.ToString());
            //JsonSerializer.SerializeToStream(error,ctx.Response.OutputStream);
        }

        static void HandleTooBusyError(IHttpContext ctx)
        {
            ctx.Response.StatusCode = (int) HttpStatusCode.ServiceUnavailable;
            ctx.Response.StatusDescription = "Service Unavailable";
            SerializeError(ctx, new
                {
                    Url = ctx.Request.RawUrl,
                    Error = "The server is too busy"
                });
        }


        public Task Start(CancellationToken token)
        {
            var virtualDirectory = _environment.VirtualDirectory ?? "";
            if (!virtualDirectory.EndsWith("/"))
            {
                virtualDirectory += "/";
            }

            var prefix = "http://" + (_environment.OptionalHostName ?? "+") + ":" + _environment.Port +
                virtualDirectory;


            return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _listener = new HttpListener();
                        _listener.Prefixes.Add(prefix);
                        _listener.Start();
                    }
                    catch (HttpListenerException ex)
                    {
                        if (ex.ErrorCode == 5)
                        {
                            var solution =
                                string.Format(
                                    "Make sure to run as admin or reserve prefix. For reservation you can try executing as admin: netsh http add urlacl url={0} user={1}\\{2}",
                                    prefix, Environment.MachineName,
                                    Environment.UserName);
                            _observer.Notify(new ConfigurationWarningEncountered("HttpListener can't connect to " + prefix + ". " + solution,
                                ex));
                        }
                        else
                        {
                            _observer.Notify(new FailedToStartHttpListener(ex, prefix));
                        }
                    }

                    catch (Exception ex)
                    {
                        _observer.Notify(new FailedToStartHttpListener(ex, prefix));
                    }
                    _listener.BeginGetContext(ar => GetContext(ar, token), null);
                    WaitTillAllProcessorsQuit(token);
                    _listener.Stop();
                }, token);
        }


        void WaitTillAllProcessorsQuit(CancellationToken token)
        {
            // first wait for the cancellation token
            token.WaitHandle.WaitOne();
            // then wait till all handlers are complete for a few seconds
            if (_requestSemaphore.CurrentCount == 0)
                return;
            var waited = 0;
            while ((_requestSemaphore.CurrentCount == 0) && (waited < 4))
            {
                _requestSemaphore.AvailableWaitHandle.WaitOne(TimeSpan.FromSeconds(0.9));
                waited += 1;
            }
        }
    }
}