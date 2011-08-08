using System.Runtime.Serialization;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class AddLogin : ISesCommand
    {
        [DataMember]
        public readonly string Username;
        [DataMember]
        public readonly string Password;

        public AddLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}