#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Build.Engine;
using Microsoft.WindowsAzure;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_Azure_Configuration : Given_atomic_storage_setup
    {
        protected override void Wire_in_the_partition(CqrsEngineBuilder builder, HandlerFactory factory)
        {
            var account = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), account);

            builder.Azure(m =>
            {
                m.AddAzureProcess(account, new[] { "test-incoming" }, c => c.QueueVisibility(1));
                m.AddAzureSender(account, "test-incoming", x => x.IdGeneratorForTests());
            });
            builder.Storage(m => m.AtomicIsInAzure(account));
        }
    }
}