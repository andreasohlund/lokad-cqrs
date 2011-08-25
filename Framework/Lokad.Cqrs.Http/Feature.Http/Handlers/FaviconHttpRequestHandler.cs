using System.ComponentModel.Composition;
using System.Globalization;
using System.Net;
using System.Resources;
using Lokad.Cqrs.Properties;

namespace Lokad.Cqrs.Feature.Http.Handlers
{
    [Export(typeof(AbstractHttpRequestHandler))]
    public sealed class FaviconHttpRequestHandler : AbstractHttpRequestHandler
    {
        public override string UrlPattern
        {
            get { return "^/favicon.ico$"; }
        }

        public override string[] SupportedVerbs
        {
            get { return new[] {"GET"}; }
        }
        
        public override void Handle(IHttpContext context)
        {
            var stream = context.Response.OutputStream;
            Resources.favicon.Save(stream);
            stream.Flush();
            context.SetStatusTo(HttpStatusCode.OK);
        }
    }
}