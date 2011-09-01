#region (c) 2010-2011 Lokad CQRS - New BSD License

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Feature.Http.Handlers;
using Lokad.Cqrs.Feature.Http;
using System.Net;
using ServiceStack.Text;

namespace Snippets.HttpEndpoint
{
    class MouseStatsRequestHandler: AbstractHttpRequestHandler
    {
        private readonly MouseStats _mouseStats;
        public MouseStatsRequestHandler(MouseStats stats)
        {
            _mouseStats = stats;
        }

        public override void Handle(IHttpContext context)
        {
            _mouseStats.RefreshStatistics();

            var data =  JsonSerializer.SerializeToString(_mouseStats);

            context.Response.ContentType = "application/json";

            context.WriteString(data);
            context.SetStatusTo(HttpStatusCode.OK);
        }

        public override string[] SupportedVerbs
        {
            get { return new[] { "GET" }; }
        }

        public override string UrlPattern
        {
            get { return "^/mousestats\\.*$"; }
        }
    }
}
