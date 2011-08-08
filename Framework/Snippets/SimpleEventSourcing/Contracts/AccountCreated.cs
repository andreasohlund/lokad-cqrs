using System.Runtime.Serialization;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class AccountCreated : ISesEvent
    {
        public readonly string Name;

        public AccountCreated(string name)
        {
            Name = name;
        }
    }
}