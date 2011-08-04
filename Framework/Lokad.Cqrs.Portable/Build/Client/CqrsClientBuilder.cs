#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Feature.MemoryPartition;
using System.Linq;

namespace Lokad.Cqrs.Build.Client
{
    public sealed class MessageLookupModule
    {
        readonly List<Assembly> _assemblies;
        readonly IList<Predicate<Type>> _constraints;

        public MessageLookupModule()
        {
            _constraints = new List<Predicate<Type>>
                {
                    type => false == type.IsAbstract
                };
            _assemblies = new List<Assembly>();
        }


        /// <summary>
        /// Includes assemblies of the specified types into the discovery process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public void InAssemblyOf<T>()
        {
            _assemblies.Add(typeof(T).Assembly);
        }

        public void InAssemblyOf(object instance)
        {
            _assemblies.Add(instance.GetType().Assembly);
        }

        public void WhereMessages(Predicate<Type> constraint)
        {
            _constraints.Add(constraint);
        }

        public IEnumerable<Type> LookupMessages()
        {
            if (_assemblies.Count == 0)
            {
                _assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(AssemblyScanEvil.IsProbablyUserAssembly)); 
            }
            return _assemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => _constraints.All(predicate => predicate(t)));
        }
    }

    public class CqrsClientBuilder : HideObjectMembersFromIntelliSense, IAdvancedClientBuilder
    {
        readonly MessageLookupModule _domain = new MessageLookupModule();
        readonly StorageModule _storageModule = new StorageModule();

        readonly List<IModule> _enlistments = new List<IModule>();

        readonly QueueWriterRegistry _registry = new QueueWriterRegistry();


        readonly List<IObserver<ISystemEvent>> _observers = new List<IObserver<ISystemEvent>>()
            {
                new ImmediateTracingObserver()
            };

        IList<IObserver<ISystemEvent>> IAdvancedClientBuilder.Observers
        {
            get { return _observers; }
        }

        void IAdvancedClientBuilder.RegisterModule(IModule module)
        {
            _enlistments.Add(module);
        }

        void IAdvancedClientBuilder.RegisterObserver(IObserver<ISystemEvent> observer)
        {
            _observers.Add(observer);
        }


        IEnvelopeSerializer _envelopeSerializer = new EnvelopeSerializerWithDataContracts();
        Func<Type[], IDataSerializer> _dataSerializer = types => new DataSerializerWithDataContracts(types);


        void IAdvancedClientBuilder.DataSerializer(Func<Type[], IDataSerializer> serializer)
        {
            _dataSerializer = serializer;
        }

        void IAdvancedClientBuilder.EnvelopeSerializer(IEnvelopeSerializer serializer)
        {
            _envelopeSerializer = serializer;
        }

        readonly ContainerBuilder _builder = new ContainerBuilder();

        void IAdvancedClientBuilder.ConfigureContainer(Action<ContainerBuilder> build)
        {
            build(_builder);
        }

        public void Storage(Action<StorageModule> configure)
        {
            configure(_storageModule);
        }


        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public void Domain(Action<MessageLookupModule> config)
        {
            config(_domain);
        }

        readonly SerializationContractRegistry _serializationList = new SerializationContractRegistry();

        public CqrsClient Build()
        {
            foreach (var module in _enlistments)
            {
                _builder.RegisterModule(module);
            }

            var container = _builder.Build();
            Configure(container.ComponentRegistry);
            return new CqrsClient(container);
        }

        void IAdvancedClientBuilder.UpdateContainer(IComponentRegistry registry)
        {
            foreach (var module in _enlistments)
            {
                _builder.RegisterModule(module);
            }
            _builder.Update(registry);
            Configure(registry);
        }

        

        void Configure(IComponentRegistry reg)
        {
            var system = new SystemObserver(_observers.ToArray());
            reg.Register<ISystemObserver>(system);
            // domain should go before serialization

            _serializationList.AddRange(_domain.LookupMessages());
            _storageModule.Configure(reg);

            var serializer = _dataSerializer(_serializationList.GetAndMakeReadOnly());
            var streamer = new EnvelopeStreamer(_envelopeSerializer, serializer);

            reg.Register(new MemoryAccount());
            reg.Register(serializer);
            reg.Register<IEnvelopeStreamer>(c => streamer);
            reg.Register(_registry);
        }

        public IAdvancedClientBuilder Advanced
        {
            get { return this; }
        }

        public void File(Action<FileClientModule> config)
        {
            var module = new FileClientModule();
            config(module);
            _builder.RegisterModule(module);
        }
    }
}