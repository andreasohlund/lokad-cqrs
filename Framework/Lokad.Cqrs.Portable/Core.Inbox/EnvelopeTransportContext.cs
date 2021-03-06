﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Core.Inbox
{
    public sealed class EnvelopeTransportContext
    {
        public readonly object TransportMessage;
        public readonly ImmutableEnvelope Unpacked;
        public readonly string QueueName;

        public EnvelopeTransportContext(object transportMessage, ImmutableEnvelope unpacked, string queueName)
        {
            TransportMessage = transportMessage;
            QueueName = queueName;
            Unpacked = unpacked;
        }
    }
}