#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Synthetic
{
    [TestFixture]
    public sealed class Given_Basic_Scenarios_When_Files : Given_Basic_Scenarios
    {
        readonly FileStorageConfig _config;

        public Given_Basic_Scenarios_When_Files()
        {
            _config = FileStorage.CreateConfig(typeof(Given_Basic_Scenarios_When_Files).Name);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _config.Wipe();
        }

        [SetUp]
        public void SetUp()
        {
            _config.Reset();
        }

        protected override void Wire_partition_to_handler(CqrsEngineBuilder config)
        {
            config.File(m =>
                {
                    m.AddFileSender(_config, "do");
                    m.AddFileProcess(_config, "do", Bootstrap);
                });
            config.Storage(s => s.AtomicIsInFiles(_config.FullPath));
        }
    }
}