using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Funq;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Feature.StreamingStorage;
using Lokad.Cqrs.Feature.TapeStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class StorageModule : HideObjectMembersFromIntelliSense
    {
        IAtomicStorageFactory _atomicStorageFactory;
        IStreamingRoot _streamingRoot;

        ITapeStorageFactory _tapeStorage;

        
        public void AtomicIs(IAtomicStorageFactory factory)
        {
            _atomicStorageFactory = factory;
        }

        //public void AtomicIs(Func<IComponentContext>)
        public void AtomicIsInMemory()
        {
            AtomicIsInMemory(builder => { });
        }

        public void AtomicIsInMemory(Action<DefaultAtomicStorageStrategyBuilder> configure)
        {
            var dictionary = new ConcurrentDictionary<string, byte[]>();
            var builder = new DefaultAtomicStorageStrategyBuilder();
            configure(builder);
            AtomicIs(new MemoryAtomicStorageFactory(dictionary, builder.Build()));
        }

        public void AtomicIsInFiles(string folder, Action<DefaultAtomicStorageStrategyBuilder> configure)
        {
            var builder = new DefaultAtomicStorageStrategyBuilder();
            configure(builder);
            AtomicIs(new FileAtomicStorageFactory(folder, builder.Build()));
        }
        public void TapeIs(ITapeStorageFactory storage)
        {
            _tapeStorage = storage;
        }

        public void TapeIsInMemory()
        {
            var storage = new ConcurrentDictionary<string, List<byte[]>>();
            var factory = new MemoryTapeStorageFactory(storage);
            TapeIs(factory);
        }

        public void TapeIsInFiles(string fullPath)
        {
            var factory = new FileTapeStorageFactory(fullPath);
            TapeIs(factory);
        }

        public void AtomicIsInFiles(string folder)
        {
            AtomicIsInFiles(folder, builder => { });
        }


        Action<Container> _modules = container => { };


        public void StreamingIsInFiles(string filePath)
        {
            _streamingRoot = new FileStreamingContainer(filePath);
        }

        public void StreamingIs(IStreamingRoot streamingRoot)
        {
            _streamingRoot = streamingRoot;
        }

        public void EnlistModule(Action<Container> module)
        {
            _modules += module;
        }


        public void Configure(Container container)
        {
            if (_atomicStorageFactory == null)
            {
                AtomicIsInMemory(strategyBuilder => { });
            }
            if (_streamingRoot == null)
            {
                StreamingIsInFiles(Directory.GetCurrentDirectory());
            }
            if (_tapeStorage == null)
            {
                TapeIsInMemory();
            }

            container.Sources.Push(new AtomicRegistrationCore());
            container.Register(new NuclearStorage(_atomicStorageFactory));

            container.Resolve<List<IEngineProcess>>()
                .Add(new AtomicStorageInitialization(new[] { _atomicStorageFactory }, container.Resolve<ISystemObserver>()));
            

            container.Register(_streamingRoot);

            container.Register(_tapeStorage);
            container.Register< IEngineProcess>(new TapeStorageInitilization(new[] { _tapeStorage }));

            
        }
    }
}