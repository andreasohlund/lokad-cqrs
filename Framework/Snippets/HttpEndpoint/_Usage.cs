using System;
using System.IO;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.Http;
using Lokad.Cqrs.Feature.Http.Handlers;
using NUnit.Framework;
using Lokad.Cqrs;

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
                c => new FaviconHttpRequestHandler());
            builder.Build().RunForever();
        }
    }
}