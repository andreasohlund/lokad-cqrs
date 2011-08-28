#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Lokad.Cqrs.Core.Outbox
{
    public sealed class QueueWriterRegistry : HideObjectMembersFromIntelliSense
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)] readonly
            ConcurrentDictionary<string, IQueueWriterFactory> _dictionary =
                new ConcurrentDictionary<string, IQueueWriterFactory>();

        public void Add(IQueueWriterFactory factory)
        {
            if (!_dictionary.TryAdd(factory.Endpoint, factory))
            {
                var message = string.Format("Failed to add {0}.{1}", factory.GetType().Name, factory.Endpoint);
                throw new InvalidOperationException(message);
            }
        }


        public IQueueWriterFactory GetOrAdd(string endpoint, Func<string, IQueueWriterFactory> factory)
        {
            return _dictionary.GetOrAdd(endpoint, factory);
        }

        public bool TryGet(string endpoint, out IQueueWriterFactory factory)
        {
            return _dictionary.TryGetValue(endpoint, out factory);
        }

        public IQueueWriterFactory GetOrThrow(string endpoint)
        {
            IQueueWriterFactory value;
            if (_dictionary.TryGetValue(endpoint, out value))
            {
                return value;
            }
            throw new InvalidOperationException(
                @"Failed to locate queue factory for '{0}'. Please ensure registration.
Normally Lokad.Cqrs tries to auto-register a factory, based on certain configs,
however this is not always the case. So you might need to do something like:

builder.Advanced.RegisterQueueWriterFactory()");
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}], x{2:X8}", GetType().Name, _dictionary.Count, GetHashCode());
        }
    }
}