#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Build.Engine
{
    public interface IAdvancedEngineBuilder : IHideObjectMembersFromIntelliSense
    {
        void RegisterQueueWriterFactory(Func<Container, IQueueWriterFactory> activator);
        void RegisterModule(IFunqlet module);
        void ConfigureContainer(Action<Container> build);
        void RegisterObserver(IObserver<ISystemEvent> observer);
        IList<IObserver<ISystemEvent>> Observers { get; }
        void CustomEnvelopeSerializer(IEnvelopeSerializer serializer);
        void CustomDataSerializer(Func<Type[], IDataSerializer> serializer);
    }
}