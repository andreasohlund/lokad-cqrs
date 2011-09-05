#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.HandlerClasses;
using StructureMap;

namespace Lokad.Cqrs
{
    public static class ExtendCqrsEngineBuilder
    {
        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="builder">configuration syntax.</param>
        /// <param name="config">The config.</param>
        public static void MessagesWithHandlersFromStructureMap(this CqrsEngineBuilder builder,
            Action<MessagesWithHandlersConfigurationSyntax> config)
        {
            builder.MessagesWithHandlers(new StructureMapContainerProvider(ObjectFactory.Container)
                .Build, config);
        }

        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="builder">configuration syntax.</param>
        /// <param name="config">The config.</param>
        /// <param name="container">A preconfigured structuremap container</param>
        public static void MessagesWithHandlersFromStructureMap(this CqrsEngineBuilder builder,
            Action<MessagesWithHandlersConfigurationSyntax> config,IContainer container)
        {
            builder.MessagesWithHandlers(new StructureMapContainerProvider(container)
                .Build, config);
        }
    }
}