#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Synthetic
{
    public abstract class Given_Basic_Scenarios
    {
        [DataContract]
        public sealed class FailingMessage : Define.Command
        {
            [DataMember]
            public int FailXTimes { get; set; }
        }

        
        protected static Action<ImmutableEnvelope> Bootstrap(Container container)
        {
            var h = new HandlerComposer();
            h.Add<FailingMessage, NuclearStorage>(SmartFailing);
            return h.BuildHandler(container);
        }

        protected static void SmartFailing(FailingMessage message, NuclearStorage storage)
        {
            var status = storage.GetSingletonOrNew<int>();
            if (status < message.FailXTimes)
            {
                storage.AddOrUpdateSingleton<int>(() => 1, i => i+1);
                throw new InvalidOperationException("Failure requested");
            }
        }


        protected abstract void Wire_partition_to_handler(CqrsEngineBuilder config);

        protected int TestSpeed = 2000;

        [Test]
        public void Then_permanent_failure_is_quarantined()
        {
            var builder = new CqrsEngineBuilder();
            Wire_partition_to_handler(builder);

            using (var source = new CancellationTokenSource())
            using (builder.TestSubscribe<EnvelopeQuarantined>(e => source.Cancel()))
            using (var engine = builder.Build())
            {
                engine.Resolve<IMessageSender>().SendOne(new FailingMessage()
                    {
                        FailXTimes = 50
                    });
                var task = engine.Start(source.Token);
                task.Wait(TestSpeed);
                Assert.IsTrue(source.IsCancellationRequested);
            }
        }

        [Test]
        public void Then_transient_failure_works()
        {
            var builder = new CqrsEngineBuilder();
            Wire_partition_to_handler(builder);

            using (var source = new CancellationTokenSource())
            using (builder.TestSubscribe<EnvelopeAcked>(e => source.Cancel()))
            using (var engine = builder.Build())
            {
                var sender = engine.Resolve<IMessageSender>();
                sender.SendOne(new FailingMessage
                    {
                        FailXTimes = 1
                    });
                var task = engine.Start(source.Token);
                task.Wait(2000);
                Assert.IsTrue(source.IsCancellationRequested);
            }
        }
    }
}