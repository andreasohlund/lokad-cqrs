#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System.Linq;
using Lokad.Cqrs.Composition.Core.Directory;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    [TestFixture]
    public sealed class When_activations_constrained_to_handler_type_with_interface : MessageDirectoryFixture
    {
        MessageActivationInfo[] Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => typeof(WhenSomethingGenericHappened) == mm.Consumer);
        }

        [Test]
        public void Only_derived_messages_are_allowed()
        {
            var expected = TestMessageTypes
                .Where(t => typeof(ISomethingHappenedEvent).IsAssignableFrom(t))
                .ToArray();

            CollectionAssert.AreEquivalent(expected, QueryAllMessageTypes(Map));
        }

        [Test]
        public void Only_single_consumer_is_allowed()
        {
            var expected = new[] {typeof(WhenSomethingGenericHappened)};
            CollectionAssert.AreEquivalent(expected, QueryDistinctConsumingTypes(Map));
        }
    }
}