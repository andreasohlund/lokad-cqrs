#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using Lokad.Cqrs.Core.Serialization;
using ServiceStack.Text;

namespace Snippets.HttpEndpoint
{
    public sealed class MyJsonSerializer : AbstractDataSerializer
    {
        public MyJsonSerializer(ICollection<Type> knownTypes) : base(knownTypes) {}


        protected override SerializerDelegate CreateSerializer(Type type)
        {
            return (instance, stream) => JsonSerializer.SerializeToStream(instance, type, stream);
        }

        protected override DeserializerDelegate CreateDeserializer(Type type)
        {
            return stream => JsonSerializer.DeserializeFromStream(type, stream);
        }
    }
}