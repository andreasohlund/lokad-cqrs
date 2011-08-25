#region (c) 2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.Http.Events
{
    public sealed class FailedToStartHttpListener : ISystemEvent
    {
        public readonly Exception Exception;
        public readonly string Prefix;

        public FailedToStartHttpListener(Exception exception, string prefix)
        {
            Exception = exception;
            Prefix = prefix;
        }

        public override string ToString()
        {
            return string.Format("Failed to start listener on {0}:{1}", Prefix, Exception);
        }
    }
}