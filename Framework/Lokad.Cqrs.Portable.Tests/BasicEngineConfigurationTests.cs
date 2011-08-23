#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Feature.MemoryPartition;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class BasicEngineConfigurationTests
    {
        // ReSharper disable InconsistentNaming

        #region Domain

        [DataContract]
        public sealed class Message1 : Define.Command
        {
            public readonly int Block;

            public Message1(int block)
            {
                Block = block;
            }
        }

        public sealed class Consumer : Define.Handle<Message1>
        {
            readonly IMessageSender _sender;

            public Consumer(IMessageSender sender)
            {
                _sender = sender;
            }

            public void Handle(Message1 message)
            {
                if (message.Block < 5)
                {
                    _sender.SendOne(new Message1(message.Block + 1));
                }
            }
        }

        #endregion

        static void TestConfiguration(Action<CqrsEngineBuilder> config)
        {
            var builder = new CqrsEngineBuilder();
            config(builder);
            using (var t = new CancellationTokenSource())
            using (builder.TestOfType<EnvelopeAcked>().Where(ea => ea.QueueName == "do")
                .Skip(5)
                .Subscribe(c => t.Cancel()))
            using (var engine = builder.Build())
            {
                engine.Start(t.Token);
                engine.Resolve<IMessageSender>().SendOne(new Message1(0));
                if (!t.Token.WaitHandle.WaitOne(5000))
                {
                    t.Cancel();
                }
                Assert.IsTrue(t.IsCancellationRequested);
            }
        }

        [Test]
        public void PartitionWithRouter()
        {
            TestConfiguration(x => x.Memory(m =>
                {
                    m.AddMemorySender("in", module => module.IdGeneratorForTests());
                    m.AddMemoryRouter("in", me => "memory:do");
                    m.AddMemoryProcess("do", Resend);
                }));
        }

        Action<ImmutableEnvelope> Resend(Container container)
        {
            var sender = container.Resolve<IMessageSender>();
            return envelope => sender.SendOne(envelope.Items[0].Content);
        }

        [Test]
        public void WithSecondaryActivator()
        {
            TestConfiguration(x =>
                {
                    x.Advanced.RegisterQueueWriterFactory(
                        c => new MemoryQueueWriterFactory(c.Resolve<MemoryAccount>(), "custom"));
                    x.Memory(m =>
                        {
                            m.AddMemorySender("in", module => module.IdGeneratorForTests());
                            m.AddMemoryRouter("in", me => "custom:do");
                            m.AddMemoryProcess("do", Resend);
                        });
                });
        }

        [Test]
        public void Direct()
        {
            TestConfiguration(x => x.Memory(m =>
                {
                    m.AddMemorySender("do", module => module.IdGeneratorForTests());
                    m.AddMemoryProcess("do", Resend);
                }));
        }

        [Test]
        public void RouterChain()
        {
            TestConfiguration(x => x.Memory(m =>
                {
                    m.AddMemorySender("in");
                    m.AddMemoryRouter("in",
                        me => (((Message1) me.Items[0].Content).Block % 2) == 0 ? "memory:do1" : "memory:do2");
                    m.AddMemoryRouter(new[] {"do1", "do2"}, me => "memory:do");
                    m.AddMemoryProcess("do", Resend);
                }));
        }
    }
}