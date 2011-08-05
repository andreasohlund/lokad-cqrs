using System;

namespace Lokad.Cqrs.Core.Dispatch
{
    /// <summary>
    /// Simple dispatcher that delegates everything to the passed action
    /// </summary>
    /// <remarks>To be replaced by a delegate probably</remarks>
    public sealed class ActionDispatcher : ISingleThreadMessageDispatcher
    {
        readonly Action<ImmutableEnvelope> _dispatcher;
        public ActionDispatcher(Action<ImmutableEnvelope> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void DispatchMessage(ImmutableEnvelope message)
        {
            _dispatcher(message);
        }

        public void Init()
        {
            
        }
    }
}