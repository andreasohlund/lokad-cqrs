using System;
using System.Collections.Generic;
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
                _set.Add("/" + f.Remove(0, ns.Length), f);
            }
        }


        public bool WillHandle(IHttpContext context)
        {
            return _set.ContainsKey(context.GetRequestUrl());
        }

        public void Handle(IHttpContext context)
        {
            var resource = _set[context.GetRequestUrl()];
            using (var r = _resourceAssembly.GetManifestResourceStream(resource))
            {
                r.CopyTo(context.Response.OutputStream);
            }
        }

        public static IHttpRequestHandler ServeFilesFromScope(object instance)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return new EmbeddedResourceHttpRequestHandler(callingAssembly, instance.GetType().Namespace);
        }
    }
}