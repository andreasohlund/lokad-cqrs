using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs;

namespace Snippets.SimpleTimerService
{
    /// <summary>
    /// the snippet is discussed in: http://groups.google.com/group/lokad/browse_thread/thread/4d39dc299037ea09
    /// This is a sample of a timer service for powering up sagas.
    /// </summary>
    sealed class SimpleTimerService : IEngineProcess
    {
        readonly IMessageSender _sender;

        public SimpleTimerService(IMessageSender sender)
        {
            _sender = sender;
            _lastChecked = DateTime.UtcNow;
        }

        public void Dispose()
        {

        }

        public void Initialize()
        {

        }

        DateTime _lastChecked;

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() => Run(token), token);
        }

        void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var current = DateTime.UtcNow;
                // normally we do either hours or days.
                // if (_lastChecked.Date != current.Date)

                // however in this case for demo-ability
                // seconds will do the trick
                if (_lastChecked.TimeOfDay.Seconds != current.TimeOfDay.Seconds)
                {
                    try
                    {
                        // we've passed a time interval
                        _sender.SendOne(new SecondPassed());
                        _lastChecked = current;
                    }
                    catch (Exception ex)
                    {
                        // if sender fails due to azure connectivity - no problem
                        // we'll keep on trying
                        Trace.WriteLine(ex);
                    }
                }
                // sleep for X minutes or till engine is stopped.
                token.WaitHandle.WaitOne(1000);
            }
        }
    }
}
