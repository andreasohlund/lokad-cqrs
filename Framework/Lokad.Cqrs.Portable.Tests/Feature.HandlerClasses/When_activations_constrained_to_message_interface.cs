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
    public sealed class When_activations_constrained_to_message_interface : MessageDirectoryFixture
    {
        MessageActivationInfo[] Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => typeof(ISomethingHappenedEvent) == mm.Message);
        }

        [Test]
        public void Only_derived_messages_are_allowed()
        {
            var derivedMessages = TestMessageTypes
                .Where(t => typeof(ISomethingHappenedEvent).IsAssignableFrom(t));

            CollectionAssert.IsSubsetOf(QueryAllMessageTypes(Map), derivedMessages);
        }

        [Test]
        public void Non_handled_derived_messages_are_prohibited()
        {
            CollectionAssert.DoesNotContain(QueryAllMessageTypes(Map), typeof(SomethingUnexpectedHandled));
        }
    }
}