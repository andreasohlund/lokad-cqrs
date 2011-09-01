using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class MiscTests
    {
        // ReSharper disable InconsistentNaming
        [Test]
        public void Azure_queues_regex_is_valid()
        {
            Assert.IsTrue(AzureEngineModule.QueueName.IsMatch("some-queue"));
            Assert.IsFalse(AzureEngineModule.QueueName.IsMatch("-some-queue"));
        } 
    }
}