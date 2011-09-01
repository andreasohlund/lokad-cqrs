#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
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
            // this test will start a simple web server on port 8082 (with a CQRS engine)
            // You might need to reserve that port or run test as admin. Check out unit test
            // output for the exact command line on port reservation (or MSDN docs)
            //
            // netsh http add urlacl url=http://+:8082/ user=RINAT-R5\Rinat.Abdullin
            // after starting the test, navigate you browser to localhost:8082/index.htm
            // and try dragging the image around

            var builder = new CqrsEngineBuilder();


            // in-memory structure to capture mouse movement
            // statistics
            var stats = new MouseStats();

            // we accept a message just of this type, using a serializer
            builder.Messages(new[] { typeof(MouseMoved) });
            builder.Advanced.CustomDataSerializer(s => new MyJsonSerializer(s));

            // let's configure our custom Http server to 
            // 1. serve resources
            // 2. serve MouseStats View
            // 3. accept commands
            var environment = new HttpEnvironment { Port = 8082 };
            builder.HttpServer(environment,
                c => new EmbeddedResourceHttpRequestHandler(typeof(MouseMoved).Assembly, "Snippets.HttpEndpoint"),
                c => new MouseStatsRequestHandler(stats),
                ConfigureMyCommandSender);

            // we'll use in-memory queues for faster processing
            // you can use files or azure in real world.
            builder.Memory(x => x.AddMemoryProcess("inbox", container => (envelope => MouseStatHandler(envelope, stats))));
            // this is a test, so let's block everything
            builder.Build().RunForever();
        }

        private static void MouseStatHandler(ImmutableEnvelope envelope, MouseStats stats)
        {
            var mouseMovedEvent = (MouseMoved)envelope.Items[0].Content;
            
            stats.MessagesCount++;

            stats.Distance += (long)Math.Sqrt(Math.Pow(mouseMovedEvent.x1 - mouseMovedEvent.x2, 2)
                                  + Math.Pow(mouseMovedEvent.y1 - mouseMovedEvent.y2, 2));
            stats.RecordMessage();
        }

        private static IHttpRequestHandler ConfigureMyCommandSender(Container c)
        {
            var writer = c.Resolve<QueueWriterRegistry>().GetOrThrow("memory").GetWriteQueue("inbox");
            return new MouseEventsRequestHandler(writer, c.Resolve<IDataSerializer>());
        }
    }
}