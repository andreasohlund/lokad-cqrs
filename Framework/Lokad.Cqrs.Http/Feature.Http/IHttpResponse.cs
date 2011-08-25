#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Specialized;
using System.IO;

namespace Lokad.Cqrs.Feature.Http
{
    public interface IHttpResponse
    {
        string RedirectionPrefix { get; set; }
        NameValueCollection Headers { get; }
        Stream OutputStream { get; }
        long ContentLength64 { get; set; }
        int StatusCode { get; set; }
        string StatusDescription { get; set; }
        string ContentType { get; set; }
        void Redirect(string url);
        void Close();
    }
}