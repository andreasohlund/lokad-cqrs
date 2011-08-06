#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.ComponentModel;


using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Container = Lokad.Cqrs.Core.Container;

namespace Lokad.Cqrs.Build
{
    using Container = Container;

    public sealed class AzureClientModule : HideObjectMembersFromIntelliSense, IFunqlet
    {
        Action<Container> _modules = context => { };




        public void AddAzureSender(IAzureStorageConfig config, string queueName, Action<SendMessageModule> configure)
        {
            var module = new SendMessageModule((context, endpoint) => new AzureQueueWriterFactory(config, context.Resolve<IEnvelopeStreamer>()), config.AccountName, queueName);
            configure(module);
            _modules += module.Configure;
        }

        

        public void AddAzureSender(IAzureStorageConfig config, string queueName)
        {
            AddAzureSender(config, queueName, m => { });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Configure(Container container)
        {
            _modules(container);
        }
    }
}