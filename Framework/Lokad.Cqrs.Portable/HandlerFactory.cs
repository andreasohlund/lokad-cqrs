using System;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs
{
    public delegate Action<ImmutableEnvelope> HandlerFactory(Container container);
}