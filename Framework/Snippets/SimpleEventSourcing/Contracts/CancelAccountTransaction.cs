using System.Runtime.Serialization;
using Snippets.SimpleEventSourcing.Definitions;

namespace Snippets.SimpleEventSourcing.Contracts
{
    [DataContract]
    public sealed class CancelAccountTransaction : ISesCommand
    {
        [DataMember] public readonly int OriginalTransactionId;
        [DataMember] public readonly string Reason;

        public CancelAccountTransaction(int originalTransactionId, string reason)
        {
            OriginalTransactionId = originalTransactionId;
            Reason = reason;
        }
    }
}