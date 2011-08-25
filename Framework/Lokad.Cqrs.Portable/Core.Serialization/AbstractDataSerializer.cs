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
    /// <summary>
    /// Base serializer class that helps with keeping a map of contract names and types
    /// </summary>
    public abstract class AbstractDataSerializer : IDataSerializer
    {
        public abstract void Serialize(object instance, Stream destinationStream);
        public abstract object Deserialize(Stream sourceStream, Type type);

        readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
        protected ICollection<Type> KnownTypes { get; private set; }
        readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();

        protected AbstractDataSerializer(ICollection<Type> knownTypes)
        {
            KnownTypes = knownTypes;
            Initialize();
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

        void Initialize()
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
            }
        }

        protected virtual string GetContractReference(Type type)
        {
            return ContractEvil.GetContractReference(type);
        }
    }
}