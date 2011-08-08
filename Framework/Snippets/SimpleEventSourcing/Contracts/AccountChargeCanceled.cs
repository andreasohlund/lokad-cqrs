using System.Runtime.Serialization;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class AccountChargeCanceled : ISesEvent
    {
        [DataMember] public int TransactionId;
        [DataMember] public decimal Amount;
        [DataMember] public decimal Balance;
        [DataMember] public string Reason;
        [DataMember] public int OriginalTransactionId;
        [DataMember] public string OriginalDescription;

        public AccountChargeCanceled(int transactionId, decimal amount, decimal balance, string reason,
            int originalTransactionId, string originalDescription)
        {
            TransactionId = transactionId;
            Amount = amount;
            Balance = balance;
            Reason = reason;
            OriginalTransactionId = originalTransactionId;
            OriginalDescription = originalDescription;
        }

        public override string ToString()
        {
            return string.Format("Cancel charge '{0}' of {1} because '{2}'. New balance {3} (tx{4}).",
                OriginalDescription, Amount, Reason, Balance, TransactionId);
        }
    }
}