#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Outbox;
using NUnit.Framework;
using System.Linq;
using System;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public abstract class Given_Atomic_Scenarios
    {
        protected abstract void Wire_in_the_partition(CqrsEngineBuilder builder);

        [DataContract]
        public sealed class AtomicMessage : Define.Command {}
        [DataContract]
        public sealed class NuclearMessage : Define.Command { }

        [Test]
        public void Then_typed_singleton_should_be_accessable_from_handler()
        {
            var builder = new CqrsEngineBuilder();
            
            using (var source = new CancellationTokenSource())
            using (Cancel_when_ok(source, builder))
            {
                Wire_in_the_partition(builder);
                using (var engine = builder.Build())
                {
                    engine.Resolve<IMessageSender>().SendOne(new AtomicMessage());
                    var task = engine.Start(source.Token);
                    task.Wait(TestSpeed);
                    Assert.IsTrue(source.IsCancellationRequested);
                }
            }
        }

        static IDisposable Cancel_when_ok(CancellationTokenSource source, CqrsEngineBuilder builder)
        {
            return builder
                .TestOfType<EnvelopeSent>()
                .Where(e => e.Attributes.Any(a => a.Key == "ok"))
                .Subscribe(e => source.Cancel());
        }

        protected static void HandleAtomic(AtomicMessage msg, IMessageSender sender, IAtomicSingletonWriter<int> arg3)
        {
            var count = arg3.AddOrUpdate(() => 1, i => i + 1);
            if (count > 2)
            {
                sender.SendBatch(new object[] {}, e => e.AddString("ok"));
                return;
            }
            sender.SendOne(new AtomicMessage());
        }
        protected static void HandleNuclear(NuclearMessage msg, IMessageSender sender, NuclearStorage storage)
        {

            var count = storage.AddOrUpdateSingleton(() => 1, i => i + 1);
            if (count >= 2)
            {
                sender.SendBatch(new object[] { }, e => e.AddString("ok"));
                return;
            }
            sender.SendOne(new NuclearMessage());
        }

        protected static Action<ImmutableEnvelope> Bootstrap(Container container)
        {
            var handling = new HandlerComposer();
            handling.Add<AtomicMessage, IMessageSender, IAtomicSingletonWriter<int>>(HandleAtomic);
            handling.Add<NuclearMessage, IMessageSender, NuclearStorage>(HandleNuclear);
            return handling.BuildHandler(container);
        }

        [Test]
        public void Then_nuclear_storage_should_be_available()
        {
            var builder = new CqrsEngineBuilder();

            using (var source = new CancellationTokenSource())
            using (Cancel_when_ok(source, builder))
            {
                Wire_in_the_partition(builder);
                using (var engine = builder.Build())
                {
                    engine.Resolve<IMessageSender>().SendOne(new NuclearMessage());
                    var task = engine.Start(source.Token);
                    task.Wait(TestSpeed);
                    Assert.IsTrue(source.IsCancellationRequested);
                }
            }
        }

        protected int TestSpeed = 2000;
    }
}