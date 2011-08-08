using System;
using Snippets.SimpleEventSourcing.Contracts;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing
{
    public sealed class AccountAggregate
    {
        readonly Action<ISesEvent> _observer;
        readonly AccountAggregateState _state;

        public AccountAggregate(Action<ISesEvent> observer, AccountAggregateState state)
        {
            _observer = observer;
            _state = state;
        }

        public void When(CreateAccount c)
        {
            Apply(new AccountCreated(c.Name));
        }


        public void When(AddAccountCharge c)
        {
            var newBalance = _state.Balance - c.Amount;
            var id = _state.NextTransactionId;
            Apply(new AccountChargeAdded(id, c.Amount, newBalance, c.Description));
        }
        public void When(AddAccountPayment a)
        {
            var newBalance = _state.Balance + a.Amount;
            var id = _state.NextTransactionId;
            Apply(new AccountPaymentAdded(id, a.Amount, newBalance, a.Description, a.PaymentMethod));
        }

        public void When(CancelAccountTransaction c)
        {
            AccountAggregateState.AccountPayment payment;
            if (_state.Payments.TryGetValue(c.OriginalTransactionId, out payment))
            {
                var id = _state.NextTransactionId;
                var newBalance = _state.Balance - payment.Amount;
                Apply(new AccountPaymentCanceled(id, payment.Amount, newBalance, c.Reason, c.OriginalTransactionId,
                    payment.Description));
                return;
            }

            AccountAggregateState.AccountCharge charge;
            if (_state.Charges.TryGetValue(c.OriginalTransactionId, out charge))
            {
                var id = _state.NextTransactionId;
                var newBalance = _state.Balance + charge.Amount;
                Apply(new AccountChargeCanceled(id, charge.Amount, newBalance, c.Reason, c.OriginalTransactionId,
                    charge.Description));
                return;
            }

            throw new InvalidOperationException(string.Format("Failed to locate transaction {0}",
                    c.OriginalTransactionId));
            
        }

        public void Execute(ISesCommand c)
        {
            RedirectToWhen.InvokeCommand(this, c);
        }

        void Apply(ISesEvent e)
        {
            _state.Apply(e);
            _observer(e);
        }
    }
}