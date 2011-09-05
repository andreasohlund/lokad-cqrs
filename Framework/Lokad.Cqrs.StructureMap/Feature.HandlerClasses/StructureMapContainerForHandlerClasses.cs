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
    /// Nested scope container adapter for Autofac used for loading handler classes
    /// and auto-wiring the dependencies.
    /// </summary>
    public sealed class StructureMapContainerForHandlerClasses : IContainerForHandlerClasses
    {
        readonly IContainer _container;
        readonly Container _lokadContainer;
        static bool lokadImportPerformed;

        public StructureMapContainerForHandlerClasses(IContainer container, Container lokadContainer)
        {
            _container = container;
            _lokadContainer = lokadContainer;
        }

        public IContainerForHandlerClasses GetChildContainer(ContainerScopeLevel level)
        {
            if (!lokadImportPerformed)
                StructureMapContainerProvider.ImportLokadTypes(_lokadContainer, _container);

            //nested child containers isn't working with SM
            if (level == ContainerScopeLevel.Item)
                return new StructureMapContainerForHandlerClasses(_container, _lokadContainer)
                    {
                        SkipDispose = true
                    };

           
            return new StructureMapContainerForHandlerClasses(_container.GetNestedContainer(), _lokadContainer);
        }

        public bool SkipDispose { get; set; }

        public object ResolveHandlerByServiceType(Type serviceType)
        {
            return _container.GetInstance(serviceType);
        }
        
        public void Dispose()
        {
            if(!SkipDispose)
                _container.Dispose();
        }


        public void ImportInstancesFromLokadContainer()
        {
            lokadImportPerformed = true;
            _container.Configure(c=>
                {

                    foreach (var service in _lokadContainer.Services)
                    {
                        var type = service.Value.GetType().GetGenericArguments()[0];
                        if (_container.Model.PluginTypes.Any(p => p.PluginType == type))
                            continue;
                       
                        c.For(type).Use((ctx) =>
                        {
                            var result = typeof(Container)
                                           .GetMethod("Resolve", new Type[0])
                                           .MakeGenericMethod(type)
                                           .Invoke(_lokadContainer, null);


                            return result;
                        });
                    }
                    
                });

            _container.AssertConfigurationIsValid();

        }
    }
}