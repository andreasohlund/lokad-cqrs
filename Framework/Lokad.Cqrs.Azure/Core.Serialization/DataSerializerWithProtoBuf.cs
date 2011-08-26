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
        }

        protected override Formatter PrepareFormatter(Type type)
        {
            var name = ContractEvil.GetContractReference(type);
            var formatter = RuntimeTypeModel.Default.CreateFormatter(type);
            return new Formatter(name, formatter.Deserialize, (o, stream) => formatter.Serialize(stream,o));
        }
    }
}