using System.Runtime.Serialization;
using Lokad.Cqrs.Scenarios.SimpleES.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class LoginAdded : ISesEvent
    {
        [DataMember]
        public readonly string Login;
        [DataMember]
        public readonly string Password;

        public LoginAdded(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}