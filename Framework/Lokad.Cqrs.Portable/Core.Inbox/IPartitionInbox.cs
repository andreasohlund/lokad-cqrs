#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Threading;

namespace Lokad.Cqrs.Core.Inbox
{
    /// <summary>
    /// Retrieves (waiting if needed) message from one or more queues
    /// </summary>
    public interface IPartitionInbox
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Init();
        /// <summary>
        /// Acks the message (removing it from the original queue).
        /// </summary>
        /// <param name="envelope">The envelope.</param>
        void AckMessage(EnvelopeTransportContext envelope);
        /// <summary>
        /// Tries to take the message, waiting for it, if needed
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="context">The context for the retrieved message.</param>
        /// <returns><em>true</em> if the message was retrieved, <em>false</em> otherwise</returns>
        bool TakeMessage(CancellationToken token, out EnvelopeTransportContext context);
        /// <summary>
        /// Tries the notify the original queue that the message was not processed.
        /// </summary>
        /// <param name="context">The context of the message.</param>
        void TryNotifyNack(EnvelopeTransportContext context);
    }
}