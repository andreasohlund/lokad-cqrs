#region (c) 2010-2011 Lokad CQRS - New BSD License 
// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/
#endregion

using System;
using System.IO;
using Lokad.Cqrs.Feature.Http.Handlers;
using Lokad.Cqrs.Feature.Http;
using Lokad.Cqrs;

namespace Snippets.HttpEndpoint
{
    public class FileResourceHttpRequestHandler: IHttpRequestHandler
    {
        public bool WillHandle(IHttpContext context)
        {
            var root = Environment.CurrentDirectory;
            var localPath = Path.Combine(root, context.Request.RawUrl.TrimStart('/').Replace("/", "\\"));

            return File.Exists(localPath);
        }

        public void Handle(IHttpContext context)
        {
            var root = Environment.CurrentDirectory;
            var localPath = Path.Combine(root, context.Request.RawUrl.TrimStart('/').Replace("/","\\"));

            GuessContentType(localPath).IfValue(c => context.Response.ContentType = c);
            using (var f = new FileStream(localPath,FileMode.Open))
            {
                f.CopyTo(context.Response.OutputStream);
            }
        }

        static Optional<string> GuessContentType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || String.IsNullOrEmpty(Path.GetExtension(fileName)) )
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
    }
}