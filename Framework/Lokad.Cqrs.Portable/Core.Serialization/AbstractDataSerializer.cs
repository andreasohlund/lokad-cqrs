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
        protected ICollection<Type> KnownTypes { get; private set; }
        readonly IDictionary<Type, Formatter> _type2Contract = new Dictionary<Type, Formatter>();
        readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();

        protected abstract Formatter PrepareFormatter(Type type);

        protected sealed class Formatter
        {
            public readonly string ContractName;
            public readonly Func<Stream, object> DeserializerDelegate;
            public readonly Action<object, Stream> SerializeDelegate;

            public Formatter(string contractName, Func<Stream, object> deserializerDelegate, Action<object, Stream> serializeDelegate)
            {
                ContractName = contractName;
                DeserializerDelegate = deserializerDelegate;
                SerializeDelegate = serializeDelegate;
            }
        }

        protected AbstractDataSerializer(ICollection<Type> knownTypes)
        {
            KnownTypes = knownTypes;
            Build();
        }

        void Build()
        {
            foreach (var type in KnownTypes)
            {
                var formatter = PrepareFormatter(type);
                try
                {
                    _contract2Type.Add(formatter.ContractName, type);
                }
                catch (ArgumentException ex)
                {
                    var msg = string.Format("Duplicate contract '{0}' being added to {1}", formatter.ContractName, GetType().Name);
                    throw new InvalidOperationException(msg, ex);
                }
                try
                {
                    _type2Contract.Add(type, formatter);
                }
                catch (ArgumentException e)
                {
                    var msg = string.Format("Duplicate type '{0}' being added to {1}", type, GetType().Name);
                    throw new InvalidOperationException(msg, e);
                }
            }
        }

        /// <summary>
        /// Deserializes the object from specified source stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <returns>
        /// deserialized object
        /// </returns>
        public object Deserialize(Stream sourceStream, Type type)
        {
            Formatter value;
            if (!_type2Contract.TryGetValue(type, out value))
            {
                var s =
                    string.Format(
                        "Can't find formatter for unknown object type '{0}'. Have you passed all known types to the constructor?",
                        type);
                throw new InvalidOperationException(s);
            }
            return value.DeserializerDelegate(sourceStream);
        }

        /// <summary>
        /// Serializes the object to the specified stream
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="type">The type to use as a serialization reference.</param>
        /// <param name="destinationStream">The destination stream.</param>
        public void Serialize(object instance, Type type, Stream destinationStream)
        {
            Formatter formatter;
            if (!_type2Contract.TryGetValue(type, out formatter))
            {
                var s =
                    string.Format(
                        "Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?",
                        instance.GetType());
                throw new InvalidOperationException(s);
            }
            formatter.SerializeDelegate(instance, destinationStream);
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
            Formatter formatter;
            if (_type2Contract.TryGetValue(messageType, out formatter))
            {
                contractName = formatter.ContractName;
                return true;
            }
            contractName = null;
            return false;
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
    }
}