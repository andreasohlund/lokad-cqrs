using System;
using Autofac;

namespace Lokad.Cqrs.Feature.DirectoryDispatch.Autofac
{
    public sealed class AutofacContainerScope : INestedContainer
    {
        readonly ILifetimeScope _scope;

        public AutofacContainerScope(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public INestedContainer GetChildContainer(ContainerScopeLevel level)
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