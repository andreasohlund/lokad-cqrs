using System;
using Autofac;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs.Feature.DirectoryDispatch.Autofac
{
    public static class AutofacContainerProvider
    {
        public static INestedContainer Build(Container container, Type[] consumers)
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