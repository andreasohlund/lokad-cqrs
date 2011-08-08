#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Snippets.PubSubRouter
{
    [TestFixture, Explicit]
    public sealed class PubSub_Usage
    {
        // ReSharper disable InconsistentNaming

        [DataContract]
        public sealed class SomethingHappened : Define.Event
        {
            
        }
        [DataContract]
        public sealed class OtherHappened : Define.Event
        {
            
        }

        [Test]
        public void Test()
        {
            var builder = new CqrsEngineBuilder();
            // only message contracts within this class
            builder.Messages(m => m.WhereMessages(t => t.DeclaringType == GetType()));

            // configure in memory:
            //                            -> sub 1 
            //  inbox -> [PubSubRouter] <
            //                            -> sub 2
            //
            builder.Memory(m =>
                {
                    m.AddMemorySender("inbox", x => x.IdGeneratorForTests());
                    m.AddMemoryProcess("inbox", x => x.DispatcherIs(ConfigureDispatcher));
                    m.AddMemoryProcess("sub1", container => (envelope => Trace.WriteLine("sub1 got")) );
                    m.AddMemoryProcess("sub2", container => (envelope => Trace.WriteLine("sub2 got")));
                });

            using (var engine = builder.Build())
            using (var cts = new CancellationTokenSource())
            {
                var task = engine.Start(cts.Token);
                var sender = engine.Resolve<IMessageSender>();

                // no handler should get these.
                sender.SendOne(new SomethingHappened());
                sender.SendOne(new OtherHappened());

                // subscribe sub1 to all messages and sub2 to specific message
                sender.SendControl(eb =>
                    {
                        eb.AddString("router-subscribe:sub1", ".*");
                        eb.AddString("router-subscribe:sub2", "SomethingHappened");
                    });
                sender.SendOne(new SomethingHappened());
                sender.SendOne(new OtherHappened());

                // unsubscribe all
                sender.SendControl(eb =>
                {
                    eb.AddString("router-unsubscribe:sub1", ".*");
                    eb.AddString("router-unsubscribe:sub2", "SomethingHappened");
                });
                sender.SendOne(new SomethingHappened());
                sender.SendOne(new OtherHappened());

                

                task.Wait(5000);

            }
        }

        static ISingleThreadMessageDispatcher ConfigureDispatcher(Container arg)
        {
            var storage = arg.Resolve<NuclearStorage>();
            var registry = arg.Resolve<QueueWriterRegistry>();
            IQueueWriterFactory factory;
            if (!registry.TryGet("memory", out factory))
            {
                throw new InvalidOperationException("Failed to get queue: memory");
            }
            return new PubSubRouter(storage, factory);
        }
    }
}