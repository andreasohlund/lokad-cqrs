#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Lokad.Cqrs.Core.Outbox
{
    public sealed class SendMessageModule : HideObjectMembersFromIntelliSense
    {
        readonly Func<Container, string,IQueueWriterFactory> _construct;
        readonly string _endpoint;
        

        readonly HashSet<string> _queueNames = new HashSet<string>();

        Func<string> _keyGenerator = () => Guid.NewGuid().ToString().ToLowerInvariant();


        public SendMessageModule(Func<Container, string, IQueueWriterFactory> construct, string endpoint, string queueName)
        {
            _construct = construct;
            _endpoint = endpoint;
            _queueNames.Add(queueName);
        }

        /// <summary>
        /// Provides customer identity generator.
        /// </summary>
        /// <param name="generator">The generator.</param>
        public void IdGenerator(Func<string> generator)
        {
            _keyGenerator = generator;
        }

        /// <summary>
        /// Allows to specify additional queues to use for random load balancing of a message
        /// </summary>
        /// <param name="queueNames">The queue names.</param>
        public void MoreRandomQueues(params string[] queueNames)
        {
            foreach (var name in queueNames)
            {
                _queueNames.Add(name);
            }
        }

        static int _counter;
        static int Domain = DateTime.Now.TimeOfDay.Minutes;

        /// <summary>
        /// Uses Id generator designed for the testing
        /// </summary>
        public void IdGeneratorForTests()
        {
            long id = 0;
            var counter = _counter++;
            var prefix = Domain.ToString("00") + "-" + counter.ToString("00") + "-";

            IdGenerator(() =>
                {
                    Interlocked.Increment(ref id);
                    return prefix + id.ToString("0000");
                });
        }

        IMessageSender BuildDefaultMessageSender(Container c)
        {
            var observer = c.Resolve<ISystemObserver>();
            var registry = c.Resolve<QueueWriterRegistry>();
            
            var factory = registry.GetOrAdd(_endpoint, s => _construct(c, s));
            var queues = _queueNames.Select(factory.GetWriteQueue).ToArray();
            return new DefaultMessageSender(queues, observer, _keyGenerator);

        }

        public void Configure(Container componentRegistry)
        {
            componentRegistry.Register(BuildDefaultMessageSender);
        }
    }
}