using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.TapeStorage;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing
{
    public static class EventSourcingMagic
    {
        public static Action<ImmutableEnvelope> BuildAggregateHandler(Container container)
        {
            var store = container.Resolve<ITapeStorageFactory>();
            var streamer = container.Resolve<IEnvelopeStreamer>();
            var registry = container.Resolve<QueueWriterRegistry>();

            // publish
            IQueueWriterFactory factory;
            if (!registry.TryGet("memory", out factory))
            {
                throw new InvalidOperationException("Failed to lookup memory factory");
            }
            var queue = factory.GetWriteQueue("inbox");

            store.InitializeForWriting();
            return envelope => ProcessMessageToAggregateRoot(queue, store, streamer, envelope);
        }

        static void ProcessMessageToAggregateRoot(
            IQueueWriter queue, 
            ITapeStorageFactory store, 
            IEnvelopeStreamer serializer,
            ImmutableEnvelope envelope)
        {
            var id = envelope.GetAttribute("to-account", "");
            if (String.IsNullOrEmpty(id))
                throw new InvalidOperationException("Only account-addressed commands are allowed");
            var stream = store.GetOrCreateStream("account-" + id);
            var state = new AccountAggregateState();
            long originalVersion = 0;
            foreach (var record in stream.ReadRecords(0, Int32.MaxValue))
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
            var envelopeId = String.Format("account-{0}-{1}x{2}", id, originalVersion, result.Count);
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
        }
    }
}