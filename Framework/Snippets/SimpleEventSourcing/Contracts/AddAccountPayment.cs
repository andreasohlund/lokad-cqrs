using System.Runtime.Serialization;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class AddAccountPayment : ISesCommand
    {
        [DataMember] public readonly decimal Amount;
        [DataMember] public readonly string Description;
        [DataMember] public readonly string PaymentMethod;

        public AddAccountPayment(decimal amount, string description, string paymentMethod)
        {
            Amount = amount;
            Description = description;
            PaymentMethod = paymentMethod;
        }
    }
}