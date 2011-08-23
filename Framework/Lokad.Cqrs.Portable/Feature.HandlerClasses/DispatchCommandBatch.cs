#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs.Core.Dispatch;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Dispatch command batches to a single consumer. Uses sliding cache to 
    /// reduce message duplication
    /// </summary>
    public class DispatchCommandBatch : ISingleThreadMessageDispatcher
    {
        readonly IDictionary<Type, Type> _messageConsumers = new Dictionary<Type, Type>();
        readonly MessageActivationInfo[] _messageDirectory;
        readonly DispatchStrategy _strategy;

        public DispatchCommandBatch(MessageActivationInfo[] messageDirectory, DispatchStrategy strategy)
        {
            _messageDirectory = messageDirectory;
            _strategy = strategy;
        }

        void ISingleThreadMessageDispatcher.DispatchMessage(ImmutableEnvelope message)
        {
            // empty message, hm...
            if (message.Items.Length == 0)
                return;

            // verify that all consumers are available
            foreach (var item in message.Items)
            {
                if (!_messageConsumers.ContainsKey(item.MappedType))
                {
                    throw new InvalidOperationException("Couldn't find consumer for " + item.MappedType);
                }
            }

            var directions = message.Items.Select(i => Tuple.Create(_messageConsumers[i.MappedType], i));
            _strategy.Dispatch(message, directions);
        }

        public void Init()
        {
            ThrowIfCommandHasMultipleConsumers(_messageDirectory);
            foreach (var messageInfo in _messageDirectory)
            {
                if (messageInfo.AllConsumers.Length > 0)
                {
                    _messageConsumers[messageInfo.MessageType] = messageInfo.AllConsumers[0];
                }
            }
        }

        static void ThrowIfCommandHasMultipleConsumers(IEnumerable<MessageActivationInfo> commands)
        {
            var multipleConsumers = commands
                .Where(c => c.AllConsumers.Length > 1)
                .Select(c => c.MessageType.FullName)
                .ToArray();

            if (!multipleConsumers.Any())
                return;

            var joined = string.Join("; ", multipleConsumers);

            throw new InvalidOperationException(
                "These messages have multiple consumers. Did you intend to declare them as events? " + joined);
        }
    }
}