#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Serializes the object to the specified stream
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="destination">The destination stream.</param>
        public override void Serialize(object instance, Stream destination)
        {
            var serializer = new DataContractSerializer(instance.GetType(), KnownTypes);

            //using (var compressed = destination.Compress(true))
            using (var writer = XmlDictionaryWriter.CreateBinaryWriter(destination, null, null, false))
            {
                serializer.WriteObject(writer, instance);
            }
        }

        /// <summary>
        /// Deserializes the object from specified source stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <returns>deserialized object</returns>
        public override object Deserialize(Stream sourceStream, Type type)
        {
            var serializer = new DataContractSerializer(type, KnownTypes);

            using (var reader = XmlDictionaryReader.CreateBinaryReader(sourceStream, XmlDictionaryReaderQuotas.Max))
            {
                return serializer.ReadObject(reader);
            }
        }
    }
}