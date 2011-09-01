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
            var builder = new CqrsEngineBuilder();

            var environment = new HttpEnvironment { Port = 8082 };

            var stats = new MouseStats();

            builder.Messages(new[] { typeof(MouseMoved) });
            builder.Advanced.CustomDataSerializer(s => new MyJsonSerializer(s));
            
            builder.HttpServer(environment,
                c => new EmbeddedResourceHttpRequestHandler(typeof(MouseMoved).Assembly, "Snippets.HttpEndpoint"),
                c => new FileResourceHttpRequestHandler(),
                c => new MouseStatsRequestHandler(stats),
                ConfigureMyCommandSender);

            builder.Memory(x => x.AddMemoryProcess("inbox", container => (envelope => MouseStatHandler(envelope, stats))));

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

    public class MouseStats
    {        
        public DateTime Tick { get; set; }

        public int MessagesCount { get; set; }
        public int MessagesPerSecond { get; set; }
        public long Distance { get; set; }

        private readonly int[] _circularBuffer = new int[60];

        public void RecordMessage()
        {
            _circularBuffer[DateTime.Now.Second] += 1;
        }

        public void RefreshStatistics()
        {
            // clears the opposite side of the message count tracking 
            // buffer
            var offset = DateTime.Now.Second;
            for (int i = 0; i < 20; i++)
            {
                var loc = (offset + 20) % 60;
                _circularBuffer[loc] = 0;
            }

            MessagesPerSecond = _circularBuffer[offset-1];
        }
        
    }
}