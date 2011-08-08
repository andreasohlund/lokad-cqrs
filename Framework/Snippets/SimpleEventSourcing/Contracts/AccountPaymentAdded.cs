using System.Runtime.Serialization;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class AccountPaymentAdded : ISesEvent
    {
        [DataMember] public readonly int TransactionId;
        [DataMember] public readonly decimal Amount;
        [DataMember] public readonly decimal Balance;
        [DataMember] public readonly string Description;
        [DataMember] public readonly string PaymentMethod;
        public AccountPaymentAdded(int transactionId, decimal amount, decimal balance, string description, string paymentMethod)
        {
            TransactionId = transactionId;
            Amount = amount;
            Balance = balance;
            Description = description;
            PaymentMethod = paymentMethod;
        }

        public override string ToString()
        {
            return string.Format("Account payment '{0}' of {1}. New balance {2} (tx{3}).", Description, Amount, Balance, TransactionId);
        }
    }
}