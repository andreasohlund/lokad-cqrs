#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Composition.Core.Directory;
using NUnit.Framework;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    [TestFixture]
    public sealed class When_activation_map_constrained_to_catch_all_consumer : MessageDirectoryFixture
    {
        MessageActivationInfo[] Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => typeof(ListenToAll) == mm.Consumer);
        }

        [Test]
        public void Only_one_consumer_is_allowed()
        {
            CollectionAssert.AreEquivalent(new[] { typeof(ListenToAll) }, Map.SelectMany(c => c.AllConsumers).Distinct());
        }


        [Test]
        public void All_messages_are_allowed()
        {
            CollectionAssert.AreEquivalent(TestMessageTypes, Map.Select(c => c.MessageType).Distinct());
        }
    }
}