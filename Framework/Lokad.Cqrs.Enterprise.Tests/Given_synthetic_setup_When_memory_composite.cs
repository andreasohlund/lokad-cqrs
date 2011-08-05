using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Synthetic;
using NUnit.Framework;

namespace Lokad.Cqrs.Enterprise
{
    [TestFixture]
    public sealed class Given_synthetic_setup_When_memory_composite : Given_synthetic_setup
    {
        // ReSharper disable InconsistentNaming

        protected override void Wire_partition_to_handler(CqrsEngineBuilder config)
        {
            config.Domain(ddd => ddd.WhereMessages(t => t==typeof(FailingMessage)));
            config.Memory(m =>
                {
                    m.AddMemorySender("do");
                    m.AddMemoryProcess("do", x => x.DispatchAsCommandBatch());
                });
        }
    }

    [TestFixture]
    public sealed class Given_synthetic_setup_When_memory_portable : Given_synthetic_setup
    {
        // ReSharper disable InconsistentNaming

        protected override void Wire_partition_to_handler(CqrsEngineBuilder config)
        {
            var h = new Handling();
            h.Add<FailingMessage>(FailAlways);
            config.Memory(m =>
            {
                m.AddMemorySender("do");
                m.AddMemoryProcess("do", h.BuildFactory());
            });
        }
    }
}