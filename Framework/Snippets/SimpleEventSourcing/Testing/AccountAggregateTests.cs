using System;
using System.Collections.Generic;
using NUnit.Framework;
using Snippets.SimpleEventSourcing.Contracts;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Testing
{
    [TestFixture]
    public sealed class AccountAggregateTests : AggregateTestSyntax<AccountAggregate>
    {
        // ReSharper disable InconsistentNaming

        protected override AccountAggregate BuildAggregate(IEnumerable<ISesEvent> events, Action<ISesEvent> observer)
        {
            var state = new AccountAggregateState();
            foreach (var sesEvent in events)
            {
                state.Apply(sesEvent);
            }
            return new AccountAggregate(observer, state);
        }

        [Test]
        public void CancelPayment()
        {
            Given(new AccountPaymentAdded(1,10,10, "payment","payment"));
            When(new CancelAccountTransaction(1, "dunno"));
            Then(new AccountPaymentCanceled(2, 10, 0, "dunni",1, "payment"));
        }

        [Test]
        public void Cancel_something_wrong()
        {
            When(new CancelAccountTransaction(1, "dunno"));
            Then(typeof(InvalidOperationException));
        }
    }
}