using System;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
{
    public interface IContainerForHandlerClasses : IDisposable
    {
        IContainerForHandlerClasses GetChildContainer(ContainerScopeLevel level);
        object Resolve(Type type);
    }
}