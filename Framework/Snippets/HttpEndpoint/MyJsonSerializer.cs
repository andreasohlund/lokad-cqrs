using System;
using System.Collections.Generic;
using System.IO;
using Lokad.Cqrs.Core.Serialization;
using ServiceStack.Text;

namespace Snippets.HttpEndpoint
{
    public sealed class MyJsonSerializer : AbstractDataSerializer
    {
        public MyJsonSerializer(ICollection<Type> knownTypes) : base(knownTypes) { }
        public override void Serialize(object instance, Stream destinationStream)
        {
            JsonSerializer.SerializeToStream(instance, instance.GetType(), destinationStream);
        }

        public override object Deserialize(Stream sourceStream, Type type)
        {
            return JsonSerializer.DeserializeFromStream(type, sourceStream);
        }
    }
}