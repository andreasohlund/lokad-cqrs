#region (c) 2010-2011 Lokad CQRS - New BSD License

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Runtime.Serialization;

namespace Snippets.HttpEndpoint
{
    [DataContract]
    public sealed class MouseMoved
    {
        public int x1 { get; set; }
        public int y1 { get; set; }
        public int x2 { get; set; }
        public int y2 { get; set; }

        public override string ToString()
        {
            return String.Format("x1: {0}, y1: {1}, x2: {2}, y2: {3}", x1, y1, x2, y2);
        }

    }
}