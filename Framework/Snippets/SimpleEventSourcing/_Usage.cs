using System;
using System.Diagnostics;
using System.Threading;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Envelope;
using NUnit.Framework;
using Snippets.SimpleEventSourcing.Contracts;
using Snippets.SimpleEventSourcing.Definitions;

// ReSharper disable InconsistentNaming
namespace Snippets.SimpleEventSourcing
{
    /// <summary>
    /// See ReadMe.markdown for the details
    /// </summary>
    [TestFixture, Explicit]
    public sealed class _Usage
    {
        [Test]
        public void Test()
        {
            var builder = new CqrsEngineBuilder();

            builder.Messages(m => m.WhereMessagesAre<ISesMessage>());
            builder.Storage(m => m.AtomicIsInMemory());
            builder.Memory(m =>
                {
                    m.AddMemorySender("inbox", c => c.IdGeneratorForTests());
                    m.AddMemoryRouter("inbox", envelope =>
                        {
                            var accountId = envelope.GetAttribute("to-account","");
                            if (!string.IsNullOrEmpty(accountId))
                            {
                                return "memory:account-aggregate";
                            }
                            return "memory:events";
                        });
                    m.AddMemoryProcess("account-aggregate", EventSourcingMagic.BuildAggregateHandler);
                    m.AddMemoryProcess("events", container => (envelope => Console.WriteLine(envelope.Items[0].Content)) );
                });

            using (var source = new CancellationTokenSource())
            using (var engine = builder.Build())
            {
                engine.Start(source.Token);

                var sender = engine.Resolve<IMessageSender>();
                sender.SendOne(new CreateAccount("Sample User"), ToAccount("1"));
                sender.SendBatch(new[]
                    {
                        new AddAccountPayment(100, "welcome bonus", "auto"),
                        new AddAccountPayment(10, "promotion bonus", "promo-code")
                    }, ToAccount("1"));

                sender.SendOne(new AddAccountCharge(75, "services charge"), ToAccount("1"));
                sender.SendOne(new CancelAccountTransaction(2, "no promotion for this account"), ToAccount("1"));
                sender.SendOne(new CancelAccountTransaction(3, "oups"), ToAccount("1"));



                source.Token.WaitHandle.WaitOne(5000);
                source.Cancel();
            }
        }

        Action<EnvelopeBuilder> ToAccount(string id)
        {
            return cb => cb.AddString("to-account", id);
        }
    }
}