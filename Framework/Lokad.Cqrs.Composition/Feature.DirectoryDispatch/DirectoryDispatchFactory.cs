using System;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Dispatch;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
{
    public static class DirectoryDispatchFactory
    {
        public static ISingleThreadMessageDispatcher CommandBatch(Container ctx, Action<MessageDirectoryFilter> optionalFilter)
        {
            var builder = ctx.Resolve<MessageDirectoryBuilder>();
            var filter = new MessageDirectoryFilter();
            optionalFilter(filter);
            
            var map = builder.BuildActivationMap(filter.DoesPassFilter);

            var strategy = ctx.Resolve<AutofacDispatchStrategy>();
            return new DispatchCommandBatch(map, strategy);
        }
        public static ISingleThreadMessageDispatcher OneEvent(Container ctx, Action<MessageDirectoryFilter> optionalFilter)
        {
            var builder = ctx.Resolve<MessageDirectoryBuilder>();
            var filter = new MessageDirectoryFilter();
            optionalFilter(filter);

            var map = builder.BuildActivationMap(filter.DoesPassFilter);

            var strategy = ctx.Resolve<AutofacDispatchStrategy>();
            var observer = ctx.Resolve<ISystemObserver>();
            return new DispatchOneEvent(map, observer, strategy);
        }
    }
}