using System;
using Funq;

namespace Lokad.Cqrs.Core.Dispatch
{
    public interface IAdvancedDispatchBuilder : IHideObjectMembersFromIntelliSense
    {
        void DispatcherIs(Func<Container, ISingleThreadMessageDispatcher> factory);
    }
}