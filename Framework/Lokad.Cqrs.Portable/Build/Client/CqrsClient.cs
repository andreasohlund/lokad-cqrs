using System;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs.Build.Client
{
    public sealed class CqrsClient : IDisposable
    {
        public Container Scope { get; private set; }

        public CqrsClient(Container scope)
        {
            Scope = scope;
        }

        public IMessageSender Sender
        {
            get
            {
                var sender = Scope.TryResolve<IMessageSender>();
                if(null != sender)
                {
                    return sender;
                }
                var message = string.Format("Failed to discover default {0}, have you added it to the system?", typeof(IMessageSender).Name);
                throw new InvalidOperationException(message);
            }
        }

        public TService Resolve<TService>()
        {
            return Scope.Resolve<TService>();
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}