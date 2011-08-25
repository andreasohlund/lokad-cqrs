#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using ProtoBuf.Meta;

namespace Lokad.Cqrs.Core.Serialization
{
    public class DataSerializerWithProtoBuf : AbstractDataSerializer
    {
        public DataSerializerWithProtoBuf(ICollection<Type> knownTypes) : base(knownTypes)
        {
            if (knownTypes.Count == 0)
                throw new InvalidOperationException(
                    "ProtoBuf requires some known types to serialize. Have you forgot to supply them?");
        }

        protected override SerializerDelegate CreateSerializer(Type type)
        {
            var formatter = RuntimeTypeModel.Default.CreateFormatter(type);
            return (instance, stream) => formatter.Serialize(stream, instance);
        }

        protected override DeserializerDelegate CreateDeserializer(Type type)
        {
            var formatter = RuntimeTypeModel.Default.CreateFormatter(type);
            return stream => formatter.Deserialize(stream);
        }
    }
}