#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Funq;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Feature.MemoryPartition;

// ReSharper disable UnusedMethodReturnValue.Global

namespace Lokad.Cqrs.Build.Engine
{
    /// <summary>
    /// Fluent API for creating and configuring <see cref="CqrsEngineHost"/>
    /// </summary>
    public class CqrsEngineBuilder : HideObjectMembersFromIntelliSense, IAdvancedEngineBuilder
    {
        readonly SerializationContractRegistry _dataSerialization = new SerializationContractRegistry();
        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer = types => new DataSerializerWithDataContracts(types);
        readonly StorageModule _storage = new StorageModule();

        Action<Container, SerializationContractRegistry> _directory;

        public CqrsEngineBuilder()
        {
            // default
            _directory = 
                (registry, contractRegistry) => { };

            _activators.Add(context => new MemoryQueueWriterFactory(context.Resolve<MemoryAccount>()));
            
        }

        readonly IList<Func<Container, IQueueWriterFactory>> _activators = new List<Func<Container, IQueueWriterFactory>>();

        readonly List<IObserver<ISystemEvent>> _observers = new List<IObserver<ISystemEvent>>
            {
                new ImmediateTracingObserver()
            };


        void IAdvancedEngineBuilder.CustomDataSerializer(Func<Type[], IDataSerializer> serializer)
        {
            _dataSerializer = serializer;
        }

        void IAdvancedEngineBuilder.CustomEnvelopeSerializer(IEnvelopeSerializer serializer)
        {
            _envelopeSerializer = serializer;
        }

        void IAdvancedEngineBuilder.RegisterQueueWriterFactory(Func<Container,IQueueWriterFactory> activator)
        {
            _activators.Add(activator);
        }

        Action<Container> _moduleEnlistments = container => { };


        void IAdvancedEngineBuilder.RegisterModule(IFunqlet module)
        {
            _moduleEnlistments += module.Configure;
        }

        
        


        void IAdvancedEngineBuilder.ConfigureContainer(Action<Container> build)
        {
            _moduleEnlistments += build;
        }

        void IAdvancedEngineBuilder.RegisterObserver(IObserver<ISystemEvent> observer)
        {
            _observers.Add(observer);
        }

        IList<IObserver<ISystemEvent>> IAdvancedEngineBuilder.Observers
        {
            get { return _observers; }
        }


        public void Memory(Action<MemoryModule> configure)
        {
            var m = new MemoryModule();
            configure(m);
            _moduleEnlistments += m.Configure;
        }

        public void File(Action<FileModule> configure)
        {
            var m = new FileModule();
            configure(m);
            _moduleEnlistments += m.Configure;
        }

        /// <summary>
        /// Adds configuration to the storage module.
        /// </summary>
        /// <param name="configure">The configure.</param>
        public void Storage(Action<StorageModule> configure)
        {
            configure(_storage);
        }


        /// <summary>
        /// Builds this <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <returns>new instance of cloud engine host</returns>
        public CqrsEngineHost Build()
        {
            // nonconditional registrations
            // System presets
            var container = new Container();
            container.Register(c =>
                {
                    return new DispatcherProcess(
                        c.Resolve<ISystemObserver>(),
                        c.Resolve<ISingleThreadMessageDispatcher>(),
                        c.Resolve<IPartitionInbox>(),
                        c.Resolve<IEnvelopeQuarantine>(),
                        c.Resolve<MessageDuplicationManager>());
                }).ReusedWithin(ReuseScope.None);
            
            
            container.Register(new MemoryAccount());

            _moduleEnlistments(container);
            var system = new SystemObserver(_observers.ToArray());

            
            
            Configure(container, system);

            var processes = container.Resolve<IEnumerable<IEngineProcess>>();
            
            var host = new CqrsEngineHost(container, system, processes);
            host.Initialize();
            return host;
        }

        void Configure(Container reg, ISystemObserver system) 
        {
            reg.Register(system);
            
            // domain should go before serialization
            _directory(reg, _dataSerialization);
            _storage.Configure(reg);

            var types = _dataSerialization.GetAndMakeReadOnly();
            var dataSerializer = _dataSerializer(types);
            var streamer = new EnvelopeStreamer(_envelopeSerializer, dataSerializer);
            
            reg.Register(BuildRegistry);
            reg.Register(dataSerializer);
            reg.Register<IEnvelopeStreamer>(c => streamer);
            reg.Register(new MessageDuplicationManager());
            reg.Register(new List<IEngineProcess>());
        }

        QueueWriterRegistry BuildRegistry(Container c) {
            var r = new QueueWriterRegistry();
                    
            foreach (var activator in _activators)
            {
                var factory = activator(c);
                r.Add(factory);
            }
            return r;
        }

        public IAdvancedEngineBuilder Advanced
        {
            get { return this; }
        }
    }
}