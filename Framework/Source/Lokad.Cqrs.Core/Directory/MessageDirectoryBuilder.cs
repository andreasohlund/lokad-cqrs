#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Directory
{
	/// <summary>
	/// Default implementation of the message directory builder
	/// </summary>
	public sealed class MessageDirectoryBuilder
	{
		readonly IEnumerable<MessageMapping> _mappings;
		readonly string _methodName;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDirectoryBuilder"/> class.
		/// </summary>
		/// <param name="mappings">The message mappings.</param>
		/// <param name="methodName">Name of the method for the invocation.</param>
		public MessageDirectoryBuilder(IEnumerable<MessageMapping> mappings, string methodName)
		{
			_mappings = mappings;
			_methodName = methodName;
		}

		public MessageDirectory BuildDirectory(Func<MessageMapping, bool> filter)
		{
			var mappings = _mappings.Where(filter);

			var consumers = mappings
				.GroupBy(x => x.Consumer)
				.Select(x =>
					{
						var directs = x
							.Where(m => m.Direct)
							.Select(m => m.Message)
							.Distinct();

						var assignables = x
							.Select(m => m.Message)
							.Where(t => directs.Any(d => d.IsAssignableFrom(t)))
							.Distinct();

						return new ConsumerInfo(x.Key, assignables.ToArray());
					})
				.ToArray();


			var messages = mappings
				.ToLookup(x => x.Message)
				.Select(x =>
				{
					var domainConsumers = x
						.Where(t => t.Consumer != typeof(MessageMapping.BusSystem))
						.Where(t => t.Consumer != typeof(MessageMapping.BusNull))
						.ToArray();

					return new MessageInfo
						{
							MessageType = x.Key,
							IsDomainMessage = x.Any(t => t.Consumer != typeof (MessageMapping.BusSystem)),
							IsSystemMessage = x.Any(t => t.Consumer == typeof (MessageMapping.BusSystem)),
							AllConsumers = domainConsumers.Select(m => m.Consumer).Distinct().ToArray(),
							DerivedConsumers = domainConsumers.Where(m => !m.Direct).Select(m => m.Consumer).Distinct().ToArray(),
							DirectConsumers = domainConsumers.Where(m => m.Direct).Select(m => m.Consumer).Distinct().ToArray(),
						};
				}).ToList();

			var includedTypes = messages
				.Select(m => m.MessageType).ToSet();

			// message directory should still include all messages for the serializers
			var orphanedMessages = _mappings
				.Where(m => !includedTypes.Contains(m.Message))
				.ToArray(m => new MessageInfo
					{
						MessageType = m.Message,
						IsDomainMessage = false,
						IsSystemMessage = false,
						AllConsumers = Type.EmptyTypes,
						DerivedConsumers = Type.EmptyTypes,
						DirectConsumers = Type.EmptyTypes
					});

			messages.AddRange(orphanedMessages);
			
			return new MessageDirectory(_methodName, consumers, messages.ToArray());
		}
	}
}