#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace Lokad.Cqrs.Feature.Http
{
    public class HttpListenerResponseAdapter : IHttpResponse
    {
        readonly HttpListenerResponse _response;

        public HttpListenerResponseAdapter(HttpListenerResponse response)
        {
            _response = response;
            OutputStream = response.OutputStream;
        }

        public string RedirectionPrefix { get; set; }

        public NameValueCollection Headers
        {
            get { return _response.Headers; }
        }

        public Stream OutputStream { get; set; }

        public long ContentLength64
        {
            get { return _response.ContentLength64; }
            set { _response.ContentLength64 = value; }
        }

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public string StatusDescription
        {
            get { return _response.StatusDescription; }
            set { _response.StatusDescription = value; }
        }

        public string ContentType
        {
            get { return _response.ContentType; }
            set { _response.ContentType = value; }
        }

        public void Redirect(string url)
        {
            _response.Redirect(RedirectionPrefix + url);
        }

        public void Close()
        {
            OutputStream.Dispose();
            _response.Close();
        }
    }
}