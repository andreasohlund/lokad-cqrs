#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace Lokad.Cqrs.Core.Serialization
{
    public abstract class AbstractDataSerializer : IDataSerializer
    {
        public delegate void SerializerDelegate(object instance, Stream stream);
        public delegate object DeserializerDelegate(Stream stream);

        protected ICollection<Type> KnownTypes { get; private set; }

        protected abstract SerializerDelegate CreateSerializer(Type type);
        protected abstract DeserializerDelegate CreateDeserializer(Type type);

        readonly IDictionary<Type, SerializerDelegate> _type2Serializer = new Dictionary<Type, SerializerDelegate>();

        readonly IDictionary<Type, DeserializerDelegate> _type2Deserializer =
            new Dictionary<Type, DeserializerDelegate>();

        readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
        readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();

        protected AbstractDataSerializer(ICollection<Type> knownTypes)
        {
            KnownTypes = knownTypes;
            InitDelegate();
        }

        void InitDelegate()
        {
            foreach (var type in KnownTypes)
            {
                var reference = GetContractReference(type);
                try
                {
                    _contract2Type.Add(reference, type);
                }
                catch (ArgumentException ex)
                {
                    var msg = string.Format("Duplicate contract '{0}' being added to {1}", reference, GetType().Name);
                    throw new InvalidOperationException(msg, ex);
                }
                try
                {
                    _type2Contract.Add(type, reference);
                }
                catch (ArgumentException e)
                {
                    var msg = string.Format("Duplicate type '{0}' being added to {1}", type, GetType().Name);
                    throw new InvalidOperationException(msg, e);
                }
                _type2Deserializer[type] = CreateDeserializer(type);
                _type2Serializer[type] = CreateSerializer(type);
            }
        }

        public object Deserialize(Stream sourceStream, Type type)
        {
            DeserializerDelegate value;
            if (!_type2Deserializer.TryGetValue(type, out value))
            {
                var s =
                    string.Format(
                        "Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?",
                        type);
                throw new InvalidOperationException(s);
            }
            return value(sourceStream);
        }

        /// <summary>
        /// Serializes the object to the specified stream
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="type">The type to use as a serialization reference.</param>
        /// <param name="destinationStream">The destination stream.</param>
        public void Serialize(object instance, Type type, Stream destinationStream)
        {
            SerializerDelegate formatter;
            if (!_type2Serializer.TryGetValue(type, out formatter))
            {
                var s =
                    string.Format(
                        "Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?",
                        instance.GetType());
                throw new InvalidOperationException(s);
            }
            formatter(instance, destinationStream);
        }

        /// <summary>
        /// Gets the contract name by the type. This name could be used for
        /// sending data across the wire or persisting it.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="contractName">Name of the contract (if found).</param>
        /// <returns>
        ///   <em>True</em> if contract name is found; <em>false</em> otherwise.
        /// </returns>
        public bool TryGetContractNameByType(Type messageType, out string contractName)
        {
            return _type2Contract.TryGetValue(messageType, out contractName);
        }

        /// <summary>
        /// Gets the type by contract name. This type could be used for
        /// serialization/deserialization of the stream.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="contractType">Type of the contract (if found).</param>
        /// <returns>
        ///   <em>True</em> if contract type is found; <em>false</em> otherwise.
        /// </returns>
        public bool TryGetContractTypeByName(string contractName, out Type contractType)
        {
            return _contract2Type.TryGetValue(contractName, out contractType);
        }


        protected virtual string GetContractReference(Type type)
        {
            return ContractEvil.GetContractReference(type);
        }
    }
}