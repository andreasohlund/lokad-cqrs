using System;
using System.Net;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.Http;
using Lokad.Cqrs.Feature.Http.Handlers;

namespace Snippets.HttpEndpoint
{
    public sealed class MyAnonymousCommandSender : AbstractHttpRequestHandler
    {
        readonly IQueueWriter _writer;
        IDataSerializer _serializer;
        public MyAnonymousCommandSender(IQueueWriter writer, IDataSerializer serializer)
        {
            _writer = writer;
            _serializer = serializer;
        }

        public override string UrlPattern
        {
            get { return "^/send/[\\w\\.-]+$"; }
        }

        public override string[] SupportedVerbs
        {
            get { return new[] { "POST", "GET"}; }
        }

        public override void Handle(IHttpContext context)
        {
            var msg = new EnvelopeBuilder(Guid.NewGuid().ToString());

            var contract = context.GetRequestUrl().Remove(0,"/send/".Length);
            Type contractType;
            if (!_serializer.TryGetContractTypeByName(contract, out contractType))
            {
                context.WriteString(string.Format("Trying to post command with unknown contract '{0}'.", contract));
                context.SetStatusTo(HttpStatusCode.BadRequest);
                return;
            }

            _writer.PutMessage(msg.Build());
            context.WriteString(string.Format(@"
Normally this should be a JSON POST, containing serialized data for {0}
but let's pretend that you successfully sent a message. Or routed it", contractType));


            context.SetStatusTo(HttpStatusCode.OK);
        }
    }
}