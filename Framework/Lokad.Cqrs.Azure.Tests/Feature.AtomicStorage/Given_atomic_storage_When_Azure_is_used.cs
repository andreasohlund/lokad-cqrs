#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_atomic_storage_When_Azure_is_used : Given_atomic_storage_setup
    {
        protected override void Wire_in_the_partition(CqrsEngineBuilder builder, HandlerFactory factory)
        {
            var account = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), account);

            builder.Azure(m =>
                {
                    m.AddAzureProcess(account, new[] {"test-incoming"}, c => c.QueueVisibility(1));
                    m.AddAzureSender(account, "test-incoming", x => x.IdGeneratorForTests());
                });
            builder.Storage(m => m.AtomicIsInAzure(account));
        }
    }
}