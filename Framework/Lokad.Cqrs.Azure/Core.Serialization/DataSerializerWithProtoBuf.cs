#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using ProtoBuf.Meta;

namespace Lokad.Cqrs.Core.Serialization
{
    public class DataSerializerWithProtoBuf : AbstractDataSerializer
    {
        readonly IDictionary<Type, IFormatter> _type2Formatter = new Dictionary<Type, IFormatter>();

        public DataSerializerWithProtoBuf(ICollection<Type> knownTypes) : base(knownTypes)
        {
            if (knownTypes.Count == 0)
                throw new InvalidOperationException(
                    "ProtoBuf requires some known types to serialize. Have you forgot to supply them?");
            InitializeFormatters(knownTypes);
        }

        void InitializeFormatters(IEnumerable<Type> knownTypes)
        {
            foreach (var type in knownTypes)
            {
                var formatter = RuntimeTypeModel.Default.CreateFormatter(type);
                _type2Formatter.Add(type, formatter);
            }
        }

        public override void Serialize(object instance, Type type, Stream destination)
        {
            IFormatter formatter;
            if (!_type2Formatter.TryGetValue(type, out formatter))
            {
                var s =
                    string.Format(
                        "Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?",
                        instance.GetType());
                throw new InvalidOperationException(s);
            }
            formatter.Serialize(destination, instance);
        }

        public override object Deserialize(Stream source, Type type)
        {
            IFormatter value;
            if (!_type2Formatter.TryGetValue(type, out value))
            {
                var s =
                    string.Format(
                        "Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?",
                        type);
                throw new InvalidOperationException(s);
            }
            return value.Deserialize(source);
        }
    }
}