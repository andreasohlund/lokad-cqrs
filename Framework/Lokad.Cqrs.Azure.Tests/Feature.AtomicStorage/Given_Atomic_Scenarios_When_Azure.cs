#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_Atomic_Scenarios_When_Azure : Given_Atomic_Scenarios
    {
        protected override void Wire_in_the_partition(CqrsEngineBuilder builder)
        {
            var account = AzureStorage.CreateConfigurationForDev();
            WipeAzureAccount.Fast(s => s.StartsWith("test-"), account);
            TestSpeed = 5000;

            builder.Azure(m =>
                {
                    m.AddAzureProcess(account, new[] {"test-incoming"}, c =>
                        {
                            c.QueueVisibility(1);
                            c.DispatcherIsLambda(Bootstrap);
                        });
                    m.AddAzureSender(account, "test-incoming", x => x.IdGeneratorForTests());
                });
            builder.Storage(m => m.AtomicIsInAzure(account));
        }
    }
}