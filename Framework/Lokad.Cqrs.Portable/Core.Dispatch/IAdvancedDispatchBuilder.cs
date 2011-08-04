using System;
using Autofac;

namespace Lokad.Cqrs.Core.Dispatch
{
    public interface IAdvancedDispatchBuilder : IHideObjectMembersFromIntelliSense
    {
        void DispatcherIs(Func<IComponentContext, ISingleThreadMessageDispatcher> factory);
    }
}