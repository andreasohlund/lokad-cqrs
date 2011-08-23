#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using Lokad.Cqrs.Composition.Core.Directory;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    [TestFixture]
    public sealed class When_activations_constrained_to_handler_type_with_type : MessageDirectoryFixture
    {
        MessageActivationInfo[] Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => typeof(WhenSomethingSpecificHappened) == mm.Consumer);
        }

        [Test]
        public void Only_single_consumer_is_allowed()
        {
            var expected = new[] {typeof(WhenSomethingSpecificHappened)};
            CollectionAssert.AreEquivalent(expected, QueryDistinctConsumingTypes(Map));
        }

        [Test]
        public void Only_specific_message_is_allowed()
        {
            var expected = new[] {typeof(SomethingSpecificHappenedEvent)};
            CollectionAssert.AreEquivalent(expected, QueryAllMessageTypes(Map));
        }
    }
}