#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Outbox;
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

            builder.Messages(new [] {typeof(UserVisited)});
            
            builder.Advanced.CustomDataSerializer(t => new MyJsonSerializer(t));
            builder.HttpServer(environment,
                c => new FaviconHttpRequestHandler(),
                c => EmbeddedResourceHttpRequestHandler.ServeFilesFromScope(this),
                ConfigureMyAnonymousCommandSender);
            builder.Memory(x => x.AddMemoryProcess("inbox", container => (envelope => Console.WriteLine("Hit!")) ));
            builder.Build().RunForever();
        }

        static IHttpRequestHandler ConfigureMyAnonymousCommandSender(Container c)
        {
            var writer = c.Resolve<QueueWriterRegistry>().GetOrThrow("memory").GetWriteQueue("inbox");
            return new MyAnonymousCommandSender(writer, c.Resolve<IDataSerializer>());
        }
    }

    
    [DataContract(Name = "user-visited")]
    public sealed class UserVisited
    {
        
    }
}