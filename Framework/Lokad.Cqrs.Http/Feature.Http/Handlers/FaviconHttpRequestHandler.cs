using System.ComponentModel.Composition;
using System.Net;

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
            Properties.Resources.favicon.Save(stream);
            stream.Flush();
            context.SetStatusTo(HttpStatusCode.OK);
        }
    }
}