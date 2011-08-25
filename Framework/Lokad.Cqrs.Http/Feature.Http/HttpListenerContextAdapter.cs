#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Net;

namespace Lokad.Cqrs.Feature.Http
{
    public class HttpListenerContextAdapter : IHttpContext
    {
        readonly HttpListenerContext _context;

        public HttpListenerContextAdapter(IHttpEnvironment environment, HttpListenerContext context)
        {
            _context = context;
            Request = new HttpListenerRequestAdapter(context.Request);
            ResponseInternal = new HttpListenerResponseAdapter(context.Response);
            Environment = environment;
        }

        public IHttpEnvironment Environment { get; private set; }

        public IHttpRequest Request { get; set; }

        protected HttpListenerResponseAdapter ResponseInternal { get; set; }

        public IHttpResponse Response
        {
            get { return ResponseInternal; }
        }


        public void TryCloseResponse()
        {
            try
            {
                ResponseInternal.OutputStream.Flush();
                ResponseInternal.OutputStream.Dispose(); // this is required when using compressing stream
                _context.Response.Close();
            }
            catch {}
        }
    }
}