#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public abstract class Given_Atomic_Scenarios
    {
        protected abstract void Wire_in_the_partition(CqrsEngineBuilder builder, HandlerFactory factory);

        [DataContract]
        public sealed class AtomicMessage : Define.Command {}

        [Test]
        public void Then_typed_singleton_should_be_accessable_from_handler()
        {
            var builder = new CqrsEngineBuilder();

            using (var source = new CancellationTokenSource())
            {
                var h = new HandlerComposer();
                h.Add<AtomicMessage, IMessageSender, IAtomicSingletonWriter<int>>((message, sender, arg3) =>
                    {
                        var count = arg3.AddOrUpdate(() => 1, i => i + 1);
                        if (count > 2)
                        {
                            source.Cancel();
                            return;
                        }
                        sender.SendOne(new AtomicMessage());
                    });

                Wire_in_the_partition(builder, h.BuildFactory());
                Send_message_and_wait_for_completion(source, builder);
            }
        }

        [Test]
        public void Then_nuclear_storage_should_be_available()
        {
            var builder = new CqrsEngineBuilder();

            using (var source = new CancellationTokenSource())
            {
                var h = new HandlerComposer();
                h.Add<AtomicMessage, IMessageSender, NuclearStorage>((message, sender, arg3) =>
                    {
                        var count = arg3.AddOrUpdateSingleton(() => 1, i => i + 1);
                        if (count >= 2)
                        {
                            source.Cancel();
                            return;
                        }
                        sender.SendOne(new AtomicMessage());
                    });

                Wire_in_the_partition(builder, h.BuildFactory());
                Send_message_and_wait_for_completion(source, builder);
            }
        }

        protected int TestSpeed = 2000;

        void Send_message_and_wait_for_completion(CancellationTokenSource source, CqrsEngineBuilder builder)
        {
            using (var engine = builder.Build())
            {
                engine.Resolve<IMessageSender>().SendOne(new AtomicMessage());
                var task = engine.Start(source.Token);
                task.Wait(TestSpeed);
                Assert.IsTrue(source.IsCancellationRequested);
            }
        }
    }
}