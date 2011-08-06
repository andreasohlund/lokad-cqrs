#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Synthetic;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Composition.Synthetic
{
    [TestFixture]
    public sealed class Given_Tx_Scenarios_When_Composite_Memory : Given_Tx_Scenarios
    {
        public sealed class Handle : Define.Handle<Act>
        {
            readonly NuclearStorage _storage;

            public Handle(NuclearStorage storage)
            {
                _storage = storage;
            }

            public void Consume(Act message)
            {
                Given_Tx_Scenarios.Consume(message, _storage);
            }
        }

        protected override void Wire_partition_to_handler(CqrsEngineBuilder config)
        {
            config.Domain(d => d.WhereMessagesAre<Act>());
            config.Memory(m =>
                {
                    m.AddMemorySender("do");
                    m.AddMemoryProcess("do", x => x.DispatchAsCommandBatch());
                });
        }
    }
}