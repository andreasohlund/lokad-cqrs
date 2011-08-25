using System;
using System.Collections.Generic;
using System.IO;

namespace Lokad.Cqrs.Core.Serialization
{
    public abstract class AbstractDataSerializer : IDataSerializer
    {
        public abstract void Serialize(object instance, Stream destinationStream);
        public abstract object Deserialize(Stream sourceStream, Type type);

        readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
        public ICollection<Type> KnownTypes { get; private set; }
        readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();

        protected AbstractDataSerializer(ICollection<Type> knownTypes)
        {
            KnownTypes = knownTypes;
            Initialize();
        }

        /// <summary>
        /// Gets the contract name by the type
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>contract name (if found)</returns>
        public bool TryGetContractNameByType(Type messageType, out string contractName)
        {
            return _type2Contract.TryGetValue(messageType, out contractName);
        }

        /// <summary>
        /// Gets the type by contract name.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <returns>type that could be used for contract deserialization (if found)</returns>
        public bool TryGetContractTypeByName(string contractName, out Type contractType)
        {
            return _contract2Type.TryGetValue(contractName, out contractType);
        }

        protected void Initialize()
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
                    var format = String.Format("Failed to add contract reference '{0}'", reference);
                    throw new InvalidOperationException(format, ex);
                }

                _type2Contract.Add(type, reference);
            }
        }

        protected virtual string GetContractReference(Type type)
        {
            return ContractEvil.GetContractReference(type);
        }
    }
}