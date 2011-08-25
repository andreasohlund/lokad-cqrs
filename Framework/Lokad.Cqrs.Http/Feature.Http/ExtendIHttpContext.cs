#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Net;

namespace Lokad.Cqrs.Feature.Http
{
    public static class ExtendIHttpContext
    {
        public static string GetRequestUrl(this IHttpContext context)
        {
            var local = context.Request.Url.LocalPath;
            var directory = context.Environment.VirtualDirectory ?? "";
            if (directory != "/" &&
                local.StartsWith(directory, StringComparison.InvariantCultureIgnoreCase))
            {
                local = local.Substring(directory.Length);
                if (local.Length == 0)
                    local = "/";
            }
            return local;
        }

        public static void SetStatusTo(this IHttpContext context, HttpStatusCode code)
        {
            context.Response.StatusCode = (int) code;
            context.Response.StatusDescription = code.ToString();
        }

        public static void WriteString(this IHttpContext context, string str)
        {
            var sw = new StreamWriter(context.Response.OutputStream);
            sw.Write(str);
            sw.Flush();
        }
    }
}