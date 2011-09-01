using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lokad.Cqrs.Feature.Http.Handlers
{
    public sealed class EmbeddedResourceHttpRequestHandler : IHttpRequestHandler
    {
        readonly Assembly _resourceAssembly;
        readonly Dictionary<string,string> _set = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase); 

        public EmbeddedResourceHttpRequestHandler(Assembly resourceAssembly, string ns)
        {
            _resourceAssembly = resourceAssembly;

            if (!string.IsNullOrEmpty(ns) && !ns.EndsWith("."))
            {
                ns = ns + ".";
            }

            var filter = _resourceAssembly
                .GetManifestResourceNames()
                .Where(n => n.StartsWith(ns));

            foreach (var f in filter)
            {
                _set.Add("." + f.Remove(0, ns.Length), f);
            }
        }


        public bool WillHandle(IHttpContext context)
        {
            return _set.ContainsKey((context.GetRequestUrl()??"").Replace('/','.'));
        }

        public void Handle(IHttpContext context)
        {
            var requestUrl = (context.GetRequestUrl() ?? "").Replace('/', '.');
            var resource = _set[requestUrl];
            
            GuessContentType(resource).IfValue(s => context.Response.ContentType = s);
            using (var r = _resourceAssembly.GetManifestResourceStream(resource))
            {
                
                r.CopyTo(context.Response.OutputStream);
            }
        }

        static Optional<string> GuessContentType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return Optional<string>.Empty;

            var path = Path.GetExtension(fileName).ToLowerInvariant();
            switch (path)
            {
                case ".htm":
                    return "text/html";
                case ".html":
                    return "text/html";
                case ".js":
                    return "text/javascript";
                default:
                    return Optional<string>.Empty;
            }
        }

        public static IHttpRequestHandler ServeFilesFromScope(object instance)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return new EmbeddedResourceHttpRequestHandler(callingAssembly, instance.GetType().Namespace);
        }
    }
}