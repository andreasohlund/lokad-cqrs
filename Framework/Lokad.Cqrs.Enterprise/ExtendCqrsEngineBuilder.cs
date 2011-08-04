using System;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Feature.DirectoryDispatch;

namespace Lokad.Cqrs
{
    public static class ExtendCqrsEngineBuilder
    {
        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public static void Domain(this CqrsEngineBuilder builder, Action<DispatchDirectoryModule> config)
        {
            throw new NotImplementedException("Add SCR reg");
            //builder.
            //_directory = (registry, contractRegistry) =>
            //    {
            //        var m = new DispatchDirectoryModule();
            //        config(m);
            //        m.Configure(registry, contractRegistry);
            //    };

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
            builder.DispatcherIs(ctx => DirectoryDispatchFactory.OneEvent(ctx, action));
            
        }

        /// <summary>
        /// <para>Wires <see cref="DispatchCommandBatch"/> implementation of <see cref="ISingleThreadMessageDispatcher"/> 
        /// into this partition. It allows dispatching multiple commands (in a single envelope) to one consumer each.</para>
        /// <para> Additional information is available in project docs.</para>
        /// </summary>
        public static void DispatchAsCommandBatch(this IAdvancedDispatchBuilder builder, Action<MessageDirectoryFilter> optionalFilter = null)
        {
            var action = optionalFilter ?? (x => { });
            builder.DispatcherIs(ctx => DirectoryDispatchFactory.CommandBatch(ctx, action));
        }
    }
}