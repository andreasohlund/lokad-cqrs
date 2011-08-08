using System;
using System.Diagnostics;
using System.Threading;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Snippets.SimpleTimerService
{
    [TestFixture, Explicit]
    public sealed class SimpleTimerService_Usage
    {
        [Test]
        public void Test()
        {
            var builder = new CqrsEngineBuilder();
            // only message contracts within this class
            builder.Messages(m => m.WhereMessages(t => t== typeof(SecondPassed)));
            
            builder.Memory(m =>
                {
                    m.AddMemorySender("inbox", x => x.IdGeneratorForTests());
                    m.AddMemoryProcess("inbox", Bootstrap);
                });

            builder.Advanced.ConfigureContainer(c =>
                {
                    var sender = c.Resolve<IMessageSender>();
                    var setup = c.Resolve<EngineSetup>();
                    setup.AddProcess(new SimpleTimerService(sender));
                });

            using (var engine = builder.Build())
            {
                engine.RunForever();
            }
        }

        // ReSharper disable InconsistentNaming

        static void WhenSecondPassed(SecondPassed p)
        {
            Trace.WriteLine("yet another second passed");
        }

        static Action<ImmutableEnvelope> Bootstrap(Container container)
        {
            var composer = new HandlerComposer();

            composer.Add<SecondPassed>(WhenSecondPassed);

            return composer.BuildHandler(container);
        }
    }
}