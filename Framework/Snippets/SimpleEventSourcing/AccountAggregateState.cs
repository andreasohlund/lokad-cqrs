using System;
using System.Collections.Generic;
using Snippets.SimpleEventSourcing.Contracts;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing
{
    public sealed class AccountAggregateState
    {

        public decimal Balance { get; private set; }
        public int NextTransactionId { get; private set; }
        public void Apply(ISesEvent e)
        {
            RedirectToWhen.InvokeOptional(this,e);
        }

        public Dictionary<int, AccountCharge> Charges { get; private set; }
        public Dictionary<int, AccountPayment> Payments { get; private set; }

        public AccountAggregateState()
        {
            Payments = new Dictionary<int, AccountPayment>();
            Charges = new Dictionary<int, AccountCharge>();
            NextTransactionId = 1;
        }

        public sealed class AccountCharge
        {
            public int TransactionId { get; set; }
            public string Description { get; set; }
            public decimal Amount { get; set; }
        }

        public sealed class AccountPayment
        {
            public int TransactionId { get; set; }
            public string Description { get; set; }
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; }
        }

        public void When(AccountChargeAdded e)
        {
            Charges.Add(e.TransactionId, new AccountCharge
            {
                TransactionId = e.TransactionId,
                Description = e.Description,
                Amount = e.Amount
            });
            StepTx(e.TransactionId);
            StepBalance(-e.Amount, e.Balance);
        }

        public void When(AccountPaymentAdded e)
        {
            Payments.Add(e.TransactionId, new AccountPayment()
            {
                TransactionId = e.TransactionId,
                Description = e.Description,
                Amount = e.Amount,
                PaymentMethod = e.PaymentMethod
            });
            StepTx(e.TransactionId);
            StepBalance(e.Amount, e.Balance);
        }

        public void When(AccountChargeCanceled e)
        {
            StepTx(e.TransactionId);
            StepBalance(e.Amount, e.Balance);
            Charges.Remove(e.OriginalTransactionId);
        }

        public void When(AccountPaymentCanceled e)
        {
            StepTx(e.TransactionId);
            StepBalance(-e.Amount, e.Balance);
            Payments.Remove(e.OriginalTransactionId);
        }


        void StepTx(int transactionId)
        {
            if (transactionId != NextTransactionId)
            {
                throw new InvalidOperationException("Accounting sanity check failure");
            }
            NextTransactionId += 1;
        }
        void StepBalance(decimal change, decimal newBalance)
        {
            if ((Balance + change) != newBalance)
            {
                throw new InvalidOperationException("Accounting sanity check failure");
            }
            Balance = newBalance;
        }
    }
}