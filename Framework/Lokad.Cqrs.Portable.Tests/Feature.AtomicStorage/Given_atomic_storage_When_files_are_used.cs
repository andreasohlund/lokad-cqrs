using System.IO;
using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_atomic_storage_When_files_are_used : Given_Atomic_Scenarios
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

        public Given_atomic_storage_When_files_are_used()
        {
            var path1 = Directory.GetCurrentDirectory();
            var path2 = typeof (Given_atomic_storage_When_files_are_used).Name;
            _config = FileStorage.CreateConfig(Path.Combine(path1, path2));
        }


        protected override void Wire_in_the_partition(CqrsEngineBuilder builder, HandlerFactory factory)
        {
            builder.Storage(m => m.AtomicIsInFiles(_config.Folder.FullName));
            builder.Memory(m =>
            {
                m.AddMemoryProcess("azure-dev", factory);
                m.AddMemorySender("azure-dev", x => x.IdGeneratorForTests());
            });
        }
    }
}