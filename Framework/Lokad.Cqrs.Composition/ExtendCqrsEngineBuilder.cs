using System;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Feature.DirectoryDispatch;
using Lokad.Cqrs.Feature.DirectoryDispatch.Autofac;
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
        public static void Domain(this CqrsEngineBuilder builder,  Action<MessagesWithHandlersConfigurationSyntax> config)
        {
            var module = new MessagesWithHandlersConfigurationSyntax(AutofacContainerProvider.Build);
            config(module);
            builder.Advanced.ConfigureContainer(c => module.Configure(c, builder.Messages));
        }
    }

    public static class ExtendIWireDispatcher
    {
        /// <summary>
        /// <para>Wires <see cref="DispatchOneEvent"/> implementation of <see cref="ISingleThreadMessageDispatcher"/> 
        /// into this partition. It allows dispatching a single event to zero or more consumers.</para>
        /// <para> Additional information is available in project docs.</para>
        /// </summary>
        public static void DispatchAsEvents(this IAdvancedDispatchBuilder builder, Action<MessageDirectoryFilter> optionalFilter = null)
        {
            var action = optionalFilter ?? (x => { });
            builder.DispatcherIs(ctx => HandlerDispatchFactory.OneEvent(ctx, action));
            
        }

        /// <summary>
        /// <para>Wires <see cref="DispatchCommandBatch"/> implementation of <see cref="ISingleThreadMessageDispatcher"/> 
        /// into this partition. It allows dispatching multiple commands (in a single envelope) to one consumer each.</para>
        /// <para> Additional information is available in project docs.</para>
        /// </summary>
        public static void DispatchAsCommandBatch(this IAdvancedDispatchBuilder builder, Action<MessageDirectoryFilter> optionalFilter = null)
        {
            var action = optionalFilter ?? (x => { });
            builder.DispatcherIs(ctx => HandlerDispatchFactory.CommandBatch(ctx, action));
        }
    }
}