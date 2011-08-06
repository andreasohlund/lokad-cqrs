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
    public sealed class Given_Atomic_Scenarios_When_Memory : Given_Atomic_Scenarios
    {
        protected override void Wire_in_the_partition(CqrsEngineBuilder builder)
        {
            builder.Memory(mm =>
                {
                    mm.AddMemorySender("do");
                    mm.AddMemoryProcess("do", Bootstrap);
                });
        }
    }
}