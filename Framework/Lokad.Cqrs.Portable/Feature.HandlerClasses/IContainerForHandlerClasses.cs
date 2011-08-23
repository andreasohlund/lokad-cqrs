using System;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    public interface IContainerForHandlerClasses : IDisposable
    {
        IContainerForHandlerClasses GetChildContainer(ContainerScopeLevel level);
        object Resolve(Type type);
    }
}