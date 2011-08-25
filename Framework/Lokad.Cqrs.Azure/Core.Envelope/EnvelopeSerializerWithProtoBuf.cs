#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Envelope
{
    public sealed class EnvelopeSerializerWithProtoBuf : IEnvelopeSerializer
    {
        public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
        {
            Serializer.Serialize(stream, contract);
        }

        public EnvelopeContract DeserializeEnvelope(Stream stream)
        {
            return Serializer.Deserialize<EnvelopeContract>(stream);
        }
    }
}