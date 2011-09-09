#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.HandlerClasses;

namespace Lokad.Cqrs
{
    public static class ExtendCqrsEngineBuilder
    {
        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="builder">configuration syntax.</param>
        /// <param name="config">The config.</param>
        public static void MessagesWithHandlersFromAutofac(this CqrsEngineBuilder builder,
            Action<MessagesWithHandlersConfigurationSyntax> config)
        {
            builder.MessagesWithHandlers((funq, classes) => AutofacContainerProvider.Build(funq, classes, new ContainerBuilder()), config);
        }

        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="builder">configuration syntax.</param>
        /// <param name="config">The config.</param>
        /// <param name="extraConfig">The extra config.</param>
        public static void MessagesWithHandlersFromAutofac(this CqrsEngineBuilder builder,
            Action<MessagesWithHandlersConfigurationSyntax> config, ContainerBuilder extraConfig)
        {
            builder.MessagesWithHandlers((funq, classes) => AutofacContainerProvider.Build(funq, classes, extraConfig), config);
        }
    }
}