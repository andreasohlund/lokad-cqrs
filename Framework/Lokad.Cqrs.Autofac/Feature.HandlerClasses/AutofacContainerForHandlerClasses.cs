#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using Autofac;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Nested scope container adapter for Autofac used for loading handler classes
    /// and auto-wiring the dependencies.
    /// </summary>
    public sealed class AutofacContainerForHandlerClasses : IContainerForHandlerClasses
    {
        readonly ILifetimeScope _lifetimeScope;

        public AutofacContainerForHandlerClasses(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public IContainerForHandlerClasses GetChildContainer(ContainerScopeLevel level)
        {
            return new AutofacContainerForHandlerClasses(_lifetimeScope.BeginLifetimeScope(level));
        }

        public object ResolveHandlerByServiceType(Type serviceType)
        {
            // we'll be resolving only at the item level
            return _lifetimeScope.Resolve(serviceType);
        }
        
        public void Dispose()
        {
            _lifetimeScope.Dispose();
        }
    }
}