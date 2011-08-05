using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using NUnit.Framework;

namespace Lokad.Cqrs.Synthetic
{
    public abstract class Given_synthetic_setup
    {
        [DataContract]
        public sealed class FailingMessage : Define.Command
        {
            
        }

        public sealed class Handler : Define.Handle<FailingMessage>
        {
            public void Consume(FailingMessage message)
            {
                FailAlways(message);
            }
        }

        protected static void FailAlways(FailingMessage message)
        {
            throw new InvalidOperationException();
        }
        

        protected abstract void Wire_partition_to_handler(CqrsEngineBuilder config);

        [Test]
        public void Then_permanent_failure_is_quarantined()
        {
            var builder = new CqrsEngineBuilder();
            Wire_partition_to_handler(builder);

            using (var source = new CancellationTokenSource())
            {

                var subject = new Subject<ISystemEvent>();
                subject.OfType<EnvelopeQuarantined>().Subscribe(e => source.Cancel());
                builder.Advanced.Observers.Add(subject);
                using (var engine = builder.Build())
                {
                    engine.Resolve<IMessageSender>().SendOne(new FailingMessage());
                    var task = engine.Start(source.Token);
                    task.Wait(2000);
                    Assert.IsTrue(source.IsCancellationRequested);
                }
            }
        }
    }
}