#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;
using System.Threading;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{

    [TestFixture]
    public sealed class When_memory_is_used : Given_atomic_storage_setup
    {
        // ReSharper disable InconsistentNaming

        protected override void Wire_in_the_partition(CqrsEngineBuilder builder, HandlerFactory storage)
        {
            builder.Memory(mm =>
                {
                    mm.AddMemorySender("do");
                    mm.AddMemoryProcess("do",storage);
                });
        }
    }

    public abstract class Given_atomic_storage_setup
    {
        protected abstract void Wire_in_the_partition(CqrsEngineBuilder builder, HandlerFactory factory);

        [DataContract]
        public sealed class AtomicMessage : Define.Command { }

        [DataContract]
        public sealed class Entity : Define.AtomicEntity
        {
            [DataMember(Order = 1)]
            public int Count;
        }

        [Test]
        public void Then_singleton_should_be_accessable_from_handler()
        {
            var builder = new CqrsEngineBuilder();

            using (var source = new CancellationTokenSource())
            {
                var h = new Handling();
                h.Add<AtomicMessage,IMessageSender,IAtomicSingletonWriter<Entity>>((message, sender, arg3) =>
                    {
                        var entity = arg3.AddOrUpdate(r => r.Count += 1);
                        if (entity.Count == 5)
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
                var h = new Handling();
                h.Add<AtomicMessage, IMessageSender, NuclearStorage>((message, sender, arg3) =>
                {
                    var entity = arg3.UpdateSingletonEnforcingNew<Entity>(r => r.Count += 1);
                    if (entity.Count == 5)
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

        static void Send_message_and_wait_for_completion(CancellationTokenSource source, CqrsEngineBuilder builder)
        {
            using (var engine = builder.Build())
            {
                var task = engine.Start(source.Token);
                engine.Resolve<IMessageSender>().SendOne(new AtomicMessage());
                task.Wait(2000);
                Assert.IsTrue(source.IsCancellationRequested);
            }
        }
    }
}