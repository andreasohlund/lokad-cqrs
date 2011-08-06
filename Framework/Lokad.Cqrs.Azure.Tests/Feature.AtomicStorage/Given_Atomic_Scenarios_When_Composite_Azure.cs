#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_Atomic_Scenarios_When_Composite_Azure : Given_Atomic_Scenarios
    {
        public sealed class AtomicHandler : Define.Handle<AtomicMessage>
        {
            readonly IAtomicSingletonWriter<int> _writer;
            readonly IMessageSender _sender;

            public AtomicHandler(IMessageSender sender, IAtomicSingletonWriter<int> writer)
            {
                _sender = sender;
                _writer = writer;
            }

            public void Consume(AtomicMessage message)
            {
                HandleAtomic(message, _sender, _writer);
            }
        }

        public sealed class NuclearHandler : Define.Handle<NuclearMessage>
        {
            readonly IMessageSender _sender;
            readonly NuclearStorage _storage;

            public NuclearHandler(IMessageSender sender, NuclearStorage storage)
            {
                _sender = sender;
                _storage = storage;
            }

            public void Consume(NuclearMessage message)
            {
                HandleNuclear(message, _sender, _storage);
            }
        }

        protected override void Wire_in_the_partition(CqrsEngineBuilder builder)
        {
            var account = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), account);
            TestSpeed = 10000;

            builder.Domain(d => d.WhereMessages(t => t.DeclaringType == GetType()));
            builder.Azure(m =>
                {
                    m.AddAzureProcess(account, new[] {"test-incoming"}, c =>
                        {
                            c.QueueVisibility(1);
                            c.DispatchAsCommandBatch();
                        });
                    m.AddAzureSender(account, "test-incoming", x => x.IdGeneratorForTests());
                });
            builder.Storage(m => m.AtomicIsInAzure(account));
        }
    }
}