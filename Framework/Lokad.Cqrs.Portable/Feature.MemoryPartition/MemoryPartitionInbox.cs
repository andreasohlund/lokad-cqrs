#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using Lokad.Cqrs.Core.Inbox;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    /// <summary>
    /// In-memory implementation of <see cref="IPartitionInbox"/> that uses concurrency primitives
    /// </summary>
    public sealed class MemoryPartitionInbox : IPartitionInbox
    {
        readonly BlockingCollection<ImmutableEnvelope>[] _queues;
        readonly string[] _names;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryPartitionInbox"/> class.
        /// </summary>
        /// <param name="queues">The queues.</param>
        /// <param name="names">Names for these queues.</param>
        public MemoryPartitionInbox(BlockingCollection<ImmutableEnvelope>[] queues, string[] names)
        {
            _queues = queues;
            _names = names;
        }

        public void Init()
        {
        }

        public void AckMessage(EnvelopeTransportContext envelope)
        {
            
        }

        public bool TakeMessage(CancellationToken token, out EnvelopeTransportContext context)
        {
            while (!token.IsCancellationRequested)
            {
                // if incoming message is delayed and in future -> push it to the timer queue.
                // timer will be responsible for publishing back.

                ImmutableEnvelope envelope;
                var result = BlockingCollection<ImmutableEnvelope>.TakeFromAny(_queues, out envelope);
                if (result >= 0)
                {
                    if (envelope.DeliverOnUtc > DateTime.UtcNow)
                    {
                        // future message
                        throw new InvalidOperationException("Message scheduling has been disabled in the code");
                    }
                    context = new EnvelopeTransportContext(result, envelope, _names[result]);
                    return true;
                }
            }
            context = null;
            return false;
        }

        public void TryNotifyNack(EnvelopeTransportContext context)
        {
            var id = (int) context.TransportMessage;

            _queues[id].Add(context.Unpacked);
        }
    }
}