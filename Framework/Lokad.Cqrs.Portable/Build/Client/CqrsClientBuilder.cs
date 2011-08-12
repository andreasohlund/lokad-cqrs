#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Envelope;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Feature.MemoryPartition;
using System.Linq;

namespace Lokad.Cqrs.Build.Client
{
    public class CqrsClientBuilder : HideObjectMembersFromIntelliSense, IAdvancedClientBuilder
    {
        readonly MessageLookupModule _domain = new MessageLookupModule();
        readonly StorageModule _storageModule = new StorageModule();

        Action<Container> _enlistments = container => { };

        readonly QueueWriterRegistry _registry = new QueueWriterRegistry();


        readonly List<IObserver<ISystemEvent>> _observers = new List<IObserver<ISystemEvent>>()
            {
                new ImmediateTracingObserver()
            };

        IList<IObserver<ISystemEvent>> IAdvancedClientBuilder.Observers
        {
            get { return _observers; }
        }

        void IAdvancedClientBuilder.RegisterModule(IFunqlet module)
        {
            _enlistments += module.Configure;
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

        void IAdvancedClientBuilder.ConfigureContainer(Action<Container> build)
        {
            _enlistments += build;
            
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

        public CqrsClient Build()
        {
            var container = new Container();
            _enlistments(container);
            Configure(container);
            return new CqrsClient(container);
        }

        void IAdvancedClientBuilder.UpdateContainer(Container registry)
        {
            _enlistments(registry);
            Configure(registry);
            
        }

        

        void Configure(Container reg)
        {
            var system = new SystemObserver(_observers.ToArray());
            reg.Register<ISystemObserver>(system);
            // domain should go before serialization
            _storageModule.Configure(reg);
            var types = _domain.LookupMessages().ToArray();
            var serializer = _dataSerializer(types);
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
            _enlistments += module.Configure;
        }
    }
}