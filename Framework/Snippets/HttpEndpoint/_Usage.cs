#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.Http;
using Lokad.Cqrs.Feature.Http.Handlers;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Snippets.HttpEndpoint
{
    [TestFixture]
    public sealed class _Usage
    {
        [Test]
        public void Test()
        {
            var builder = new CqrsEngineBuilder();

            var environment = new HttpEnvironment
                {
                    Port = 8082
                };
            builder.HttpServer(environment,
                c => new FaviconHttpRequestHandler(),
                c => EmbeddedResourceHttpRequestHandler.FromScope(this));
            builder.Build().RunForever();
        }
    }
}