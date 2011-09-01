#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Static class capable of building nested container provider
    /// </summary>
    public static class AutofacContainerProvider
    {
        public static IContainerForHandlerClasses Build(Container container, Type[] handlerTypes)
        {
            var autofacBuilder = new ContainerBuilder();

            foreach (var handlerType in handlerTypes)
            {
                autofacBuilder.RegisterType(handlerType);
            }
            // allow handlers to resolve items from the core container
            autofacBuilder.RegisterSource(new FunqAdapterForAutofac(container));

            var autofacContainer = autofacBuilder.Build();
            container.Register(autofacContainer);
            return new AutofacContainerForHandlerClasses(autofacContainer);
        }
    }
}