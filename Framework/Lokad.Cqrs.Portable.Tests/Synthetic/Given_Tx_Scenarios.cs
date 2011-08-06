#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Synthetic
{
    public abstract class Given_Tx_Scenarios
    {
        [DataContract]
        public sealed class Act : Define.Command
        {
            [DataMember]
            public bool Fail { get; set; }
        }

        protected static void Consume(Act message, NuclearStorage storage)
        {
            new TransactionTester
                {
                    OnCommit = () => storage.AddOrUpdateSingleton(() => 1, i => i + 1)
                };

            if (message.Fail)
                throw new InvalidOperationException("Fail requested");
        }

        protected Action<ImmutableEnvelope> Bootstrap(Container container)
        {
            var composer = new HandlerComposer(() => new TransactionScope(TransactionScopeOption.RequiresNew));
            composer.Add<Act, NuclearStorage>(Consume);
            return composer.BuildHandler(container);
        }

        public int TestSpeed = 5000;


        [Test]
        public void Then_transactional_support_is_provided()
        {
            var builder = new CqrsEngineBuilder();

            Wire_partition_to_handler(builder);

            using (var source = new CancellationTokenSource())
            using (builder.TestSubscribe<EnvelopeAcked>(e => source.Cancel()))
            using (var engine = builder.Build())
            {
                var sender = engine.Resolve<IMessageSender>();
                sender.SendBatch(new[] {new Act(), new Act(), new Act {Fail = true}});
                sender.SendBatch(new[] {new Act(), new Act(), new Act()});

                var task = engine.Start(source.Token);
                task.Wait(TestSpeed);
                Assert.IsTrue(source.IsCancellationRequested);

                var count = engine.Resolve<NuclearStorage>().GetSingletonOrNew<int>();
                Assert.AreEqual(3, count, "Three acts are expected");
            }
        }

        protected abstract void Wire_partition_to_handler(CqrsEngineBuilder builder);
    }
}