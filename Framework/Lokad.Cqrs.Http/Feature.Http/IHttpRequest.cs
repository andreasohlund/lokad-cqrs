#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Specialized;
using System.IO;

namespace Lokad.Cqrs.Feature.Http
{
    public interface IHttpRequest
    {
        NameValueCollection Headers { get; }
        Stream InputStream { get; }
        NameValueCollection QueryString { get; }
        string HttpMethod { get; }
        Uri Url { get; set; }
        string RawUrl { get; set; }
    }
}