using System;
using Autofac;
using Lokad.Cqrs.Feature.HandlerClasses;

namespace Lokad.Cqrs.Feature.DirectoryDispatch.Autofac
{
    /// <summary>
    /// Nested scope adapter for Autofac
    /// </summary>
    public sealed class AutofacContainerScope : IContainerForHandlerClasses
    {
        readonly ILifetimeScope _scope;

        public AutofacContainerScope(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public IContainerForHandlerClasses GetChildContainer(ContainerScopeLevel level)
        {
            return new AutofacContainerScope(_scope.BeginLifetimeScope(level));
        }

        public object Resolve(Type serviceType)
        {
            return _scope.Resolve(serviceType);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}