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
    public sealed class Given_Atomic_Scenarios_When_Files : Given_Atomic_Scenarios
    {
        readonly FileStorageConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config.Wipe();
            _config.EnsureDirectory();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _config.Wipe();
        }

        public Given_Atomic_Scenarios_When_Files()
        {
            _config = FileStorage.CreateConfig(typeof(Given_Atomic_Scenarios_When_Files).Name);
        }

        protected override void Wire_in_the_partition(CqrsEngineBuilder builder)
        {
            builder.Storage(m => m.AtomicIsInFiles(_config.Folder.FullName));
            builder.Memory(m =>
                {
                    m.AddMemoryProcess("azure-dev", Bootstrap);
                    m.AddMemorySender("azure-dev", x => x.IdGeneratorForTests());
                });
        }
    }
}