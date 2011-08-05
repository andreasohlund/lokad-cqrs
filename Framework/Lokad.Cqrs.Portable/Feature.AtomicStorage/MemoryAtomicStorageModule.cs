#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System.Collections.Concurrent;
using Funq;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class MemoryAtomicStorageModule
    {
        readonly IAtomicStorageStrategy _strategy;

        public MemoryAtomicStorageModule(IAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
        }


        public void Configure(Container container)
        {
            container.Register(_strategy);
            var store = new ConcurrentDictionary<string, byte[]>();
            container.Register(store);
            container.Register<IAtomicStorageFactory>(new MemoryAtomicStorageFactory(store, _strategy));
        }
    }
}