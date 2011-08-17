#region (c) 2010-2011 Lokad CQRS - New BSD License

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Net;
using System.Web;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.Http;
using Lokad.Cqrs.Feature.Http.Handlers;
using ServiceStack.Text;

namespace Snippets.HttpEndpoint
{
    public sealed class MouseEventsRequestHandler : AbstractHttpRequestHandler
    {
        private readonly IQueueWriter _writer;
        private readonly IDataSerializer _serializer;

        public MouseEventsRequestHandler(IQueueWriter writer, IDataSerializer serializer)
        {
            _writer = writer;
            _serializer = serializer;
        }

        public override void Handle(IHttpContext context)
        {
            var envelopeBuilder = new EnvelopeBuilder("mouse-move - " + DateTime.Now.Ticks.ToString());

            var contract = context.GetRequestUrl().Remove(0, "/mouseevents/".Length);
            Type contractType;
            if (!_serializer.TryGetContractTypeByName(contract, out contractType))
            {
                context.WriteString(string.Format("Trying to post command with unknown contract '{0}'.", contract));
                context.SetStatusTo(HttpStatusCode.BadRequest);
                return;
            }

            var decodedData = HttpUtility.UrlDecode(context.Request.QueryString.ToString());
            var mouseMove =  JsonSerializer.DeserializeFromString<MouseMoved>(decodedData);

            envelopeBuilder.AddItem(mouseMove);
            _writer.PutMessage(envelopeBuilder.Build());

            context.SetStatusTo(HttpStatusCode.OK);
        }

        public override string UrlPattern
        {
            get { return "^/mouseevents/[\\w\\.-]+$"; }
        }

        public override string[] SupportedVerbs
        {
            get { return new[] { "GET" }; }
        }
    }
}