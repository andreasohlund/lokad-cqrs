#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Feature.HandlerClasses;
using Lokad.Cqrs.Feature.MemoryPartition;

// ReSharper disable UnusedMethodReturnValue.Global

namespace Lokad.Cqrs.Build.Engine
{
    /// <summary>
    /// Fluent API for creating and configuring <see cref="CqrsEngineHost"/>
    /// </summary>
    public class CqrsEngineBuilder : HideObjectMembersFromIntelliSense, IAdvancedEngineBuilder
    {
        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer;
        readonly StorageModule _storage = new StorageModule();
        readonly SystemObserver _observer;
        public CqrsEngineBuilder()
        {

            // init time observer
            _observer = new SystemObserver(new ImmediateTracingObserver());
            _setup = new EngineSetup(_observer);

            _activators.Add(context => new MemoryQueueWriterFactory(context.Resolve<MemoryAccount>()));
            _dataSerializer = types => new DataSerializerWithDataContracts(types);
        }

        /// <summary>
        /// Lightweight message configuration that wires in message contract classes.
        /// </summary>
        /// <param name="config">The config.</param>
        public void Messages(Action<MessagesConfigurationSyntax> config)
        {
            var mlm = new MessagesConfigurationSyntax();
            config(mlm);
            _serializationTypes.AddRange(mlm.LookupMessages());
        }

        /// <summary>
        /// Lightweight message configuration that wires in message contract classes.
        /// </summary>
        /// <param name="messageTypes">The message types.</param>
        public void Messages(IEnumerable<Type> messageTypes)
        {
            _serializationTypes.AddRange(messageTypes);
        }


        /// <summary>
        /// Heavy-weight configuration that discovers and wires in both message contracts 
        /// and messages handlers in an enterprise service bus style. Use <em>Messages</em>
        /// overload, if you want to use just lean and fast lambda delegates.
        /// </summary>
        /// <param name="factory">The factory that will add in a your favorite container.</param>
        /// <param name="config">The configuration for dispatch directory module.</param>
        public void MessagesWithHandlers(BuildsContainerForMessageHandlerClasses factory,
            Action<MessagesWithHandlersConfigurationSyntax> config)
        {
            var module = new MessagesWithHandlersConfigurationSyntax(factory);
            config(module);
            Advanced.ConfigureContainer(c => module.Configure(c, t => _serializationTypes.AddRange(t)));
        }

        readonly IList<Func<Container, IQueueWriterFactory>> _activators =
            new List<Func<Container, IQueueWriterFactory>>();

        readonly List<IObserver<ISystemEvent>> _observers = new List<IObserver<ISystemEvent>>
            {
                new ImmediateTracingObserver()
            };


        void IAdvancedEngineBuilder.CustomDataSerializer(Func<Type[], IDataSerializer> serializer)
        {
            _dataSerializer = serializer;
        }

        public EngineSetup Setup
        {
            get { return _setup; }
        }

        void IAdvancedEngineBuilder.CustomEnvelopeSerializer(IEnvelopeSerializer serializer)
        {
            _envelopeSerializer = serializer;
        }

        void IAdvancedEngineBuilder.RegisterQueueWriterFactory(Func<Container, IQueueWriterFactory> activator)
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

        readonly EngineSetup _setup;


        /// <summary>
        /// Builds this <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <returns>new instance of cloud engine host</returns>
        public CqrsEngineHost Build()
        {
            // nonconditional registrations
            // System presets
            var container = new Container();

            container.Register(_setup);
            container.Register(new MemoryAccount());

            _setup.Observer.Swap(_observers.ToArray());
            container.Register<ISystemObserver>(_observer);
            Configure(container);
            _moduleEnlistments(container);

            var host = new CqrsEngineHost(container, _setup.Observer, _setup.GetProcesses());
            host.Initialize();
            return host;
        }

        readonly List<Type> _serializationTypes = new List<Type>();

        void Configure(Container reg)
        {
            // domain should go before serialization
            _storage.Configure(reg);

            if (_serializationTypes.Count == 0)
            {
                // default scan if nothing specified
                Messages(m => { });
            }
            if (_serializationTypes.Count == 0)
            {
                _observer.Notify(new ConfigurationWarningEncountered("No message contracts provided."));
            }
            var dataSerializer = _dataSerializer(_serializationTypes.ToArray());
            var streamer = new EnvelopeStreamer(_envelopeSerializer, dataSerializer);

            reg.Register(BuildRegistry);
            reg.Register(dataSerializer);
            reg.Register<IEnvelopeStreamer>(c => streamer);
            reg.Register(new MessageDuplicationManager());
        }

        QueueWriterRegistry BuildRegistry(Container c)
        {
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