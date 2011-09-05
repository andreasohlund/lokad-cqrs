#region (c) 2010-2011 Lokad CQRS - New BSD License

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Linq;
using StructureMap;
using Container = Lokad.Cqrs.Core.Container;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Class capable of building nested container provider
    /// </summary>
    public class StructureMapContainerProvider
    {
        readonly IContainer strutureMapContainer;

        public StructureMapContainerProvider(IContainer strutureMapContainer)
        {
            this.strutureMapContainer = strutureMapContainer;
        }

        public IContainerForHandlerClasses Build(Container container, Type[] handlerTypes)
        {
            var containerHandler = new StructureMapContainerForHandlerClasses(strutureMapContainer, container);

            strutureMapContainer.Configure(c =>
                {
                    foreach (var handlerType in handlerTypes)
                        c.For(handlerType);
                });

            ImportLokadTypes(container,strutureMapContainer);

            container.Register(strutureMapContainer);

            return containerHandler;
        }

        public static void ImportLokadTypes(Container container, IContainer strutureMapContainer)
        {
            strutureMapContainer.Configure(c =>
                {

                    foreach (var service in container.Services)
                    {
                        var type = service.Value.GetType().GetGenericArguments()[0];
                        if (strutureMapContainer.Model.PluginTypes.Any(p => p.PluginType == type))
                            continue;

                        c.For(type).Use((ctx) =>
                            {
                                var result = typeof(Container)
                                    .GetMethod("Resolve", new Type[0])
                                    .MakeGenericMethod(type)
                                    .Invoke(container, null);


                                return result;
                            });
                    }

                });
        }

    }
}