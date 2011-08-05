using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class When_memory_is_used : Given_atomic_storage_setup
    {
        protected override void Wire_in_the_partition(CqrsEngineBuilder builder, HandlerFactory storage)
        {
            builder.Memory(mm =>
                {
                    mm.AddMemorySender("do");
                    mm.AddMemoryProcess("do",storage);
                });
        }
    }
}