#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lokad.Cqrs;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Snippets.PubSubRouter
{
    /// <summary>
    /// See ReadMe.markdown for details and <see cref="_Usage"/> for wiring
    /// </summary>
    class PubSubRouter : ISingleThreadMessageDispatcher
    {
        readonly NuclearStorage _storage;


        public void DispatchMessage(ImmutableEnvelope message)
        {
            // We either accept commands, events or control messages.


            // if tis is control message
            var controls = message
                .GetAllAttributes()
                .Where(ia => ia.Key.StartsWith("router-"))
                .ToArray();

            if (controls.Length > 0)
            {
                if (message.Items.Length > 0)
                {
                    throw new InvalidOperationException("Router control message should not have any content");
                }
                _storage.UpdateSingletonEnforcingNew<RouteTable>(rt => UpgradeRouterTable(controls, rt));
                ReloadRouteMap();
                return;
            }

            // replace with your logic to detect commands.
            if (message.Items.All(m => m.Content is Define.Command))
            {
                _queueFactory.GetWriteQueue("commands").PutMessage(message);
                return;
            }
            if (message.Items.All(m => m.Content is Define.Event))
            {
                PublishEvent(message);
                return;
            }

            throw new InvalidOperationException(
                "This is not command, event or control message. Please handle this case.");
        }

        static void UpgradeRouterTable(ImmutableAttribute[] controls, RouteTable rt)
        {
            foreach (var attrib in controls)
            {
                var key = attrib.Key;
                const string subscribePrefix = "router-subscribe:";
                if (key.StartsWith(subscribePrefix))
                {
                    var queueName = key.Remove(0, subscribePrefix.Length).Trim();
                    var regex = attrib.Value;
                    rt.Subscribe(regex, queueName);
                    continue;
                }
                const string unsubscribePrefix = "router-unsubscribe:";
                if (key.StartsWith(unsubscribePrefix))
                {
                    var queueName = key.Remove(0, unsubscribePrefix.Length).Trim();
                    var regex = attrib.Value;
                    rt.Unsubscribe(regex, queueName);
                    continue;
                }
                const string s =
                    @"Unexpected control header: '{0}' with value '{1}'.

To control this router, send messages without any content and with headers.

To subscribe:
    key:    router-subscribe:QUEUE-NAME
    value:  REGEX

To unsubscribe:
    key:    router-unsubscribe:QUEUE-NAME
    value:  REGEX";
                var message = string.Format(s, attrib.Key, attrib.Value);
                throw new InvalidOperationException(message);
            }
        }

        void ReloadRouteMap()
        {
            _routing.Clear();
            var table = _storage.GetSingletonOrNew<RouteTable>();

            foreach (var item in table.Items)
            {
                var regex = new Regex(item.ContractRegex, RegexOptions.Singleline);
                _routing.Add(Tuple.Create(regex, (ICollection<string>) item.QueueNames));
            }
        }

        readonly IList<Tuple<Regex, ICollection<string>>> _routing = new List<Tuple<Regex, ICollection<string>>>();
        readonly IQueueWriterFactory _queueFactory;

        public PubSubRouter(NuclearStorage storage, IQueueWriterFactory queueFactory)
        {
            _storage = storage;
            _queueFactory = queueFactory;
        }


        void PublishEvent(ImmutableEnvelope e)
        {
            if (1 != e.Items.Length)
            {
                throw new InvalidOperationException(
                    @"Events that go through the router can't be batched! If you are publishing
event sourcing messages, break them into the separate envelopes.");
            }
            // usual approach is to route based on the 'topic' field, which is
            // set explicitly by the sender
            // while migrating to AMQP servers, this functionality will be
            // supported by the infrastructure
            var name = e.Items[0].MappedType.FullName;
            var queues = _routing
                .Where(t => t.Item1.IsMatch(name))
                .SelectMany(m => m.Item2)
                .Distinct()
                .Select(_queueFactory.GetWriteQueue);
            // if server crashes here, that's no problem, since we'll resend later
            // and duplicates will be handled by the infrastructure
            foreach (var queue in queues)
            {
                queue.PutMessage(e);
            }
        }

        public void Init()
        {
            ReloadRouteMap();
        }
    }
}