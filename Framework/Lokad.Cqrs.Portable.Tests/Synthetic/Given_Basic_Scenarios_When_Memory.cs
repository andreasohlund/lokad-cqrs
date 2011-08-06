#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_Basic_Scenarios_When_Memory : Given_Basic_Scenarios
    {
        protected override void Wire_partition_to_handler(CqrsEngineBuilder config)
        {
            config.Memory(m =>
                {
                    m.AddMemorySender("do");
                    m.AddMemoryProcess("do", Bootstrap);
                });
        }
    }
}