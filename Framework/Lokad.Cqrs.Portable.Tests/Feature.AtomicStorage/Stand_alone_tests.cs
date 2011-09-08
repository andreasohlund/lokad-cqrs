using System;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture, Explicit]
    public sealed class Stand_alone_tests
    {
        // ReSharper disable InconsistentNaming
        [Test]
        public void Test()
        {
            
            var nuclearStorage = FileStorage.CreateNuclear(GetType().Name);

            var writer = nuclearStorage.Factory.GetEntityWriter<unit,Dunno>();
            writer.UpdateEnforcingNew(unit.it, dunno => dunno.Count += 1);
            writer.UpdateEnforcingNew(unit.it, dunno => dunno.Count += 1);

            var count = nuclearStorage.Factory.GetEntityReader<unit,Dunno>().GetOrNew().Count;
            Console.WriteLine(count);
        }

        [DataContract]
        public sealed class Dunno
        {
            [DataMember]
            public int Count { get; set; }
        }
    }
}