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
            return new AutofacContainerForHandlerClasses(autofacContainer);
        }
    }
}