#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Outbox
{
    public sealed class EnvelopeSent : ISystemEvent
    {
        public readonly string QueueName;
        public readonly string EnvelopeId;
        public readonly bool Transactional;
        public readonly string[] MappedTypes;
        public readonly ICollection<ImmutableAttribute> Attributes; 

        public EnvelopeSent(string queueName, string envelopeId, bool transactional, string[] mappedTypes, ICollection<ImmutableAttribute> attributes)
        {
            QueueName = queueName;
            EnvelopeId = envelopeId;
            Transactional = transactional;
            MappedTypes = mappedTypes;
            Attributes = attributes;
        }

        public override string ToString()
        {
            return string.Format("Sent {0}{1} to '{2}' as [{3}]", 
                string.Join("+", MappedTypes), 
                Transactional ? " +tx" : "",
                QueueName, 
                EnvelopeId);
        }
    }
}