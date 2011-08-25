#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;

namespace Lokad.Cqrs.Feature.Http
{
    public class HttpListenerRequestAdapter : IHttpRequest
    {
        readonly HttpListenerRequest _request;

        readonly NameValueCollection _queryString;

        public HttpListenerRequestAdapter(HttpListenerRequest request)
        {
            _request = request;
            _queryString = HttpUtility.ParseQueryString(Uri.UnescapeDataString(request.Url.Query));
            Url = _request.Url;
            RawUrl = _request.RawUrl;
        }

        public NameValueCollection Headers
        {
            get { return _request.Headers; }
        }

        public Stream InputStream
        {
            get { return _request.InputStream; }
        }

        public NameValueCollection QueryString
        {
            get { return _queryString; }
        }

        public Uri Url { get; set; }

        public string HttpMethod
        {
            get { return _request.HttpMethod; }
        }

        public string RawUrl { get; set; }
    }
}