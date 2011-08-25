#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.Http;
using Lokad.Cqrs.Feature.Http.Handlers;

namespace Lokad.Cqrs
{
    public static class ExtendCqrsEngineBuilderWithHttp
    {
        public static void HttpServer(this CqrsEngineBuilder builder, IHttpEnvironment environment,
            params Func<Container, AbstractHttpRequestHandler>[] handlers)
        {
            builder.Advanced.ConfigureContainer(c =>
                {
                    var instances = handlers.Select(x => x(c));
                    builder.Advanced.Setup.AddProcess(new Listener(environment, builder.Setup.Observer, instances));
                });
        }
    }
}