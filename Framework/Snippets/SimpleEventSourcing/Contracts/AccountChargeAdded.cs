using System.Runtime.Serialization;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class AccountChargeAdded : ISesEvent
    {
        [DataMember] public readonly decimal Amount;
        [DataMember] public readonly string Description;
        [DataMember] public readonly int TransactionId;
        [DataMember] public readonly decimal Balance;

        public AccountChargeAdded(int transactionId, decimal amount, decimal balance, string description)
        {
            Amount = amount;
            Description = description;
            TransactionId = transactionId;
            Balance = balance;
        }

        public override string ToString()
        {
            return string.Format("Account charge '{0}' of {1}. New balance {2} (tx{3})", Description, Amount, Balance, TransactionId);
        }
    }
}