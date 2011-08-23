using System;
using Autofac;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs.Feature.DirectoryDispatch.Autofac
{
    /// <summary>
    /// Static class capable of building nested container provider
    /// </summary>
    public static class AutofacContainerProvider
    {
        public static IContainerForHandlerClasses Build(Container container, Type[] consumers)
        {
            var autofacBuilder = new ContainerBuilder();
            foreach (var consumer in consumers)
            {
                autofacBuilder.RegisterType(consumer);
            }
            autofacBuilder.RegisterSource(new AutofacRegistrationSource(container));

            var autofacContainer = autofacBuilder.Build();
            return new AutofacContainerScope(autofacContainer);
        }
    }
}