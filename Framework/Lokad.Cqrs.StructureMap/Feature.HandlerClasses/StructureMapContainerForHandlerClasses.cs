#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using StructureMap;
using Container = Lokad.Cqrs.Core.Container;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Nested scope container adapter for StructureMap used for loading handler classes
    /// and auto-wiring the dependencies.
    /// </summary>
    public sealed class StructureMapContainerForHandlerClasses : IContainerForHandlerClasses
    {
        readonly IContainer _container;
        readonly Container _lokadContainer;

        public StructureMapContainerForHandlerClasses(IContainer container, Container lokadContainer)
        {
            _container = container;
            _lokadContainer = lokadContainer;
        }

        public IContainerForHandlerClasses GetChildContainer(ContainerScopeLevel level)
        {
            //nested child containers isn't working with SM 
            //http://groups.google.com/group/structuremap-users/browse_frm/thread/5b981676ad386417
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
    }
}