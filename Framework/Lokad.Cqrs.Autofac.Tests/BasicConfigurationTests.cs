using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    [TestFixture]
    public class BasicConfigurationTests
    {
        [Test]
        public void CanBeConfiguredWithHandlerSample()
        {
            var builder = new CqrsEngineBuilder();
            builder.MessagesWithHandlersFromAutofac(
                c => c.HandlerSample<Define.Handle<Define.Command>>(h => h.Handle(null)));

            using (builder.Build()) {}
        }
    }
}
