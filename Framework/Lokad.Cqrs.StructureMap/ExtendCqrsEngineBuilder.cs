#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Linq;
using System.Reactive.Subjects;
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
            MessagesWithHandlersFromStructureMap(builder,config,ObjectFactory.Container);
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

            var subject = builder.Advanced.Observers
              .Where(t => typeof(IObservable<ISystemEvent>).IsAssignableFrom(t.GetType()))
              .Cast<IObservable<ISystemEvent>>()
              .FirstOrDefault();

            if (null == subject)
            {
                var s = new Subject<ISystemEvent>();
                subject = s;
                builder.Advanced.Observers.Add(s);
            }


                        var provider = new StructureMapContainerProvider(container,subject);

            builder.MessagesWithHandlers(provider
                .Build, config);


        }
    }
}