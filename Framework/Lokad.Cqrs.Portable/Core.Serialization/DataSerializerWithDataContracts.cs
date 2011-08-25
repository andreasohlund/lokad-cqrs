#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace Lokad.Cqrs.Core.Serialization
{
    /// <summary>
    /// Message serializer for the <see cref="DataContractSerializer"/>. It is the default serializer in Lokad.CQRS.
    /// </summary>
    public class DataSerializerWithDataContracts : AbstractDataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSerializerWithDataContracts"/> class.
        /// </summary>
        /// <param name="knownTypes">The known types.</param>
        public DataSerializerWithDataContracts(ICollection<Type> knownTypes) : base(knownTypes)
        {
            ThrowOnMessagesWithoutDataContracts(KnownTypes);
        }

        static void ThrowOnMessagesWithoutDataContracts(IEnumerable<Type> knownTypes)
        {
            var failures = knownTypes
                .Where(m => false == m.IsDefined(typeof(DataContractAttribute), false))
                .ToList();

            if (failures.Any())
            {
                var list = String.Join(Environment.NewLine, failures.Select(f => f.FullName).ToArray());

                throw new InvalidOperationException(
                    "All messages must be marked with the DataContract attribute in order to be used with DCS: " + list);
            }
        }

        protected override SerializerDelegate CreateSerializer(Type type)
        {
            var serializer = new DataContractSerializer(type, KnownTypes);
            return (instance, stream) =>
                {
                    using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
                    {
                        serializer.WriteObject(writer, instance);
                    }
                };
        }

        protected override DeserializerDelegate CreateDeserializer(Type type)
        {
            var serializer = new DataContractSerializer(type, KnownTypes);
            return (stream) =>
                {
                    using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
                    {
                        return serializer.ReadObject(reader);
                    }
                };
        }
    }
}