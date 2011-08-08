using System.Runtime.Serialization;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class AddAccountCharge : ISesCommand
    {
        [DataMember] public readonly decimal Amount;
        [DataMember] public readonly string Description;

        public AddAccountCharge(decimal amount, string description)
        {
            Amount = amount;
            Description = description;
        }
    }
}