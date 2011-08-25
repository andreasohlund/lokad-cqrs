#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Linq;
using System.Text.RegularExpressions;

namespace Lokad.Cqrs.Feature.Http.Handlers
{
    public abstract class AbstractHttpRequestHandler
    {
        protected readonly Regex UrlMatcher;

        protected AbstractHttpRequestHandler()
        {
            UrlMatcher = new Regex(UrlPattern);
        }

        public abstract string UrlPattern { get; }
        public abstract string[] SupportedVerbs { get; }

        public bool WillHandle(IHttpContext context)
        {
            if (SupportedVerbs.Contains(context.Request.HttpMethod) == false)
                return false;
            var input = context.GetRequestUrl();
            var match = UrlMatcher.Match(input);
            return match.Success;
        }

        public abstract void Handle(IHttpContext context);
    }
}