using System;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Feature.HandlerClasses;

namespace Lokad.Cqrs
{
    public static class ExtendIAdvancedDispatchBuilder
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