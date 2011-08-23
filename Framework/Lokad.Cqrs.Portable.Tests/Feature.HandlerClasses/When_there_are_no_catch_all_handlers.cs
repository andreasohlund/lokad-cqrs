#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Composition.Core.Directory;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.HandlerClasses
{
    [TestFixture]
    public sealed class When_there_are_no_catch_all_handlers : MessageDirectoryFixture
    {
        
        MessageActivationInfo[] Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => mm.Consumer != typeof (ListenToAll));
        }

        [Test]
        public void Orphaned_messages_are_excluded()
        {
            CollectionAssert.DoesNotContain(QueryAllMessageTypes(Map), typeof (NonHandledCommand));
        }
    }
}