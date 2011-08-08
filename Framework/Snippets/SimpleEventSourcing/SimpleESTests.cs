using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.TapeStorage;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;
using NUnit.Framework;
using Snippets.SimpleEventSourcing;
using Snippets.SimpleEventSourcing.Contracts;

namespace Lokad.Cqrs.Scenarios.SimpleES
{
    [TestFixture, Explicit]
    public sealed class SimpleESTests
    {
        // ReSharper disable InconsistentNaming

        [Test]
        public void Test()
        {
            var builder = new CqrsEngineBuilder();

            builder.Messages(m => m.WhereMessagesAre<ISesMessage>());
            builder.Storage(m => m.AtomicIsInMemory());
            builder.Memory(m =>
                {
                    m.AddMemorySender("inbox", c => c.IdGeneratorForTests());
                    m.AddMemoryRouter("inbox", envelope =>
                        {
                            var accountId = envelope.GetAttribute("to-account","");
                            if (!string.IsNullOrEmpty(accountId))
                            {
                                return "memory:account-aggregate";
                            }
                            return "memory:events";
                        });
                    m.AddMemoryProcess("account-aggregate", BuildAggregateHandler);
                    m.AddMemoryProcess("events", HandlerComposer.Empty);
                });

            using (var source = new CancellationTokenSource())
            using (var engine = builder.Build())
            {
                engine.Start(source.Token);

                var sender = engine.Resolve<IMessageSender>();
                sender.SendOne(new CreateAccount("Sample User"), cb => cb.AddString("to-account", "1"));
                sender.SendOne(new AddLogin("login", "pass"), cb => cb.AddString("to-account", "1"));

                source.Token.WaitHandle.WaitOne(5000);
                source.Cancel();
            }
        }

        static Action<ImmutableEnvelope> BuildAggregateHandler(Container container)
        {
            var store = container.Resolve<ITapeStorageFactory>();
            var serializer = container.Resolve<IEnvelopeStreamer>();
            var registry = container.Resolve<QueueWriterRegistry>();

            // publish
            IQueueWriterFactory factory;
            if (!registry.TryGet("memory", out factory))
            {
                throw new InvalidOperationException("Failed to lookup memory factory");
            }
            var queue = factory.GetWriteQueue("inbox");

            store.InitializeForWriting();
            return envelope =>
                {
                    var id = envelope.GetAttribute("to-account", "");
                    if (string.IsNullOrEmpty(id))
                        throw new InvalidOperationException("Only account-addressed commands are allowed");
                    var stream = store.GetOrCreateStream("account-" + id);
                    var state = new AccountAggregateState();
                    long originalVersion = 0;
                    foreach (var record in stream.ReadRecords(0, int.MaxValue))
                    {
                        var data = serializer.ReadAsEnvelopeData(record.Data);
                        foreach (var eventItem in data.Items)
                        {
                            state.Apply((ISesEvent) eventItem.Content);
                        }
                        originalVersion = record.Version;
                    }

                    // apply 
                    var result = new List<ISesEvent>();
                    var aggregate = new AccountAggregate(result.Add, state);
                    foreach (var commandItem in envelope.Items)
                    {
                        aggregate.Execute((ISesCommand) commandItem.Content);
                    }
                    if (result.Count == 0)
                    {
                        return;
                    }
                    var then = result
                        .Select(e => new MessageBuilder(e.GetType(), e))
                        .ToList();

                    // save
                    var envelopeId = string.Format("account-{0}-{1}x{2}", id, originalVersion, result.Count);
                    var b = new EnvelopeBuilder(envelopeId);
                    foreach (var e in then)
                    {
                        b.Items.Add(e);
                    }
                    var saved = serializer.SaveEnvelopeData(b.Build());
                    var appended = stream.TryAppend(saved, TapeAppendCondition.VersionIs(originalVersion));
                    if (!appended)
                        throw new InvalidOperationException(
                            "Data was modified concurrently, and we don't have merging implemented, yet");

                    for (int i = 0; i < then.Count; i++)
                    {
                        var builder = new EnvelopeBuilder(envelopeId + "-" + i);
                        builder.Items.Add(then[i]);
                        queue.PutMessage(builder.Build());
                    }
                };
        }
    }
}