using System;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    public delegate IContainerForHandlerClasses BuildsContainerForMessageHandlerClasses(Container funq, Type[] handlerClasses);
}