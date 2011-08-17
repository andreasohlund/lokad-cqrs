---
layout: post
title: Chapter 08 - Advanced Configuration Options
---

# Advanced Configuration Options
There are some features of Lokad.CQRS that are not essential for getting started with distributed systems. Yet, they might come in handy, if you are building complex systems or just want to provide some custom behaviors.

## Configurable Message Interfaces
By default, you can derive your message contracts and handlers from the Define interfaces declared within the `Lokad.Cqrs` namespace. This speeds up the development, but introduces additional coupling between your code and Lokad.CQRS libraries.

You can avoid such coupling (and this is the recommended approach). In order to do that, you need to define your own message and handler marker interfaces and then show them to Lokad.CQRS. You can either copy-n-paste Define.cs class or write something like this:

	public interface IMyMessage {}

	public interface IMyHandler<TMessage> where TMessage : IMyMessage
	{
		void Consume(TMessage message);
	}

Then, just show Lokad.CQRS how these two come together.

	builder.Domain(m =>
		{
			m.HandlerSample<IMyHandler<IMyMessage>>(c => c.Consume(null));
		});
  
  
  
## Message Context Factories
Sometimes, while writing message handlers, you might need to access transport-level information from the actual message envelope. For example, you might want to use sending date, save the entire envelope or reference its unique identifier.

Here's where message context comes in. Message context is a strongly-typed class, which contains transport-specific information and is available to your message handlers on demand. To access it, just ask for `Func<MessageContext>` in the constructor of your message handler:

	public sealed class DoSomething : IConsume<VipMessage>, IConsume<UsualMessage>
	{
		Func<MessageContext> _context;

		public DoSomething(Func<MessageContext> context)
		{
			_context = context;
		}

		void Print(string value)
		{
			var context = _context();
			Trace.WriteLine(string.Format("Value: {0}; Id: {1}; Created: {2}", 
				value, context.EnvelopeId, context.CreatedUtc));
		}

		public void Consume(UsualMessage message)
		{
			Print(message.Word);
		}

		public void Consume(VipMessage message)
		{
			Print(message.Word);
		}
	}

You can also inject your own message context class, which could provide access to some additional transport headers in a strongly-typed and uniform fashion. It is simple to do and will also allow avoiding coupling between your handlers and Lokad.CQRS code. 

First, you define your own class to hold the message context:

	public sealed class MyContext
	{
		public readonly string MessageId;
		public readonly string Token;
		public readonly DateTimeOffset Created;

		public MyContext(string messageId, string token, DateTimeOffset created)
		{
			MessageId = messageId;
			Token = token;
			Created = created;
		}
	}

And then let's wire it in the builder syntax:

	static MyContext BuildContextOnTheFly(ImmutableEnvelope envelope, ImmutableMessage item)
	{
		var messageId = string.Format("[{0}]-{1}", envelope.EnvelopeId, item.Index);
		var token = envelope.GetAttribute("token", "");
		return new MyContext(messageId, token, envelope.CreatedOnUtc);
	}

	builder.Domain(m =>
		{
			m.HandlerSample<IMyHandler<IMyMessage>>(c => c.Consume(null));
			m.ContextFactory(BuildContextOnTheFly);
		});

In order to include custom transport headers to the outgoing messages, you just need to provide additional configuration within the message sender classes:

	IMessageSender sender = ...;
	sender.SendOne(myMessage, cb => cb.AddString("token","NeverDoThisInYourCode_SDAJ12"));

	
## Custom Envelope and Message Serialization Options
The default behavior of Lokad.CQRS is to use `DataContractSerializer` for serializing and deserializing both the message envelopes and the actual message data.

This behavior can be customized to support other serialization formats, which might be desired to achieve better performance and lower friction of managing different versions of the same contract across large distributed system.

Custom serializers could be specified using the builder syntax for both the Client and Application Engine. For example:

	builder.Advanced.DataSerializer(t => new DataSerializerWithProtoBuf(t));
	builder.Advanced.EnvelopeSerializer(new EnvelopeSerializerWithProtoBuf());

We recommend using [Google Protocol Buffers for .NET] (http://code.google.com/p/protobuf-net/), as implemented by Marc Gravell. ProtoBuf is a special cross-platform binary serialization format, which was designed by Google to handle their immense messaging needs. It is the fastest and most compact serialization under .NET framework.

If you are using Lokad.Cqrs.Azure.dll in your project, then ProtoBuf serializers are already included and could be plugged in via a short-cut:

	builder.UseProtoBufSerialization();

## Envelope Quarantine and Customizations
Message envelope handling code might fail sometimes. Message envelopes might even contain data that always causes the handler to fail. 

In order to distinguish between transient failures (i.e.: something that it caused by a network outage) and poisons (something that always fails), Lokad.CQRS features a quarantine mechanism.

Quarantine component is responsible for determining how to handle failing message envelope, and when to give up on processing it (and how to do that).

The default implementation is a `MemoryQuarantine`, which works with the majority of simple cases. It merely says that: discard message envelope, if it has caused more than 4 failures.

However, since the default behavior might not be the one desired, it can customized by implementing your own quarantine component (that derives from `IEnvelopeQuarantine`) and wiring it into the engine:

	m.AddAzureProcess(dev, IdFor.Events, x =>
		{
			x.DirectoryFilter(f => f.WhereMessagesAre<IDomainEvent>());
			x.DispatchAsEvents();
			x.Quarantine(c => new SalescastQuarantine(mail, c.Resolve<IStreamingRoot>()));
		});

## Custom Message Envelope Dispatchers
Lokad.CQRS provides only a limited number of message envelope dispatchers targeting the most common usage scenarios. However, there could be scenarios when custom dispatch behaviors are needed. For example, you might want to route message envelopers based on content and log them for audit at the same time.

Then the simplest way in this case is to write a custom dispatcher, and then register it in the partition.

Here's a sample snippet that injects custom dispatcher into the publishing partition. Note, how we construct new dispatcher instance with the services retrieved from the IoC Container.

	  m.AddAzureProcess(dev, IdFor.Publish, x =>
		  {
			  x.DispatcherIs((context, infos, strategy) =>
				  {
					  var log = new DomainLogWriter(sql);
					  var streamer = context.Resolve<IEnvelopeStreamer>();
					  var factory = context.Resolve<QueueWriterRegistry>();
					  return new MyPublishDispatcher(factory, log, PartitionCount, streamer);
				  });          
		  });

You can use following dispatcher classes (from Lokad.CQRS sources) as a sample for writing your own message dispatcher:

* DispatchCommandBatch
* DispatchOneEvent
* DispatchMessagesToRoute

## Portability Scenarios
At the moment of writing, Lokad.CQRS allows running the same application code with minimal modifications both in Windows Azure and under local development environment outside of Azure Compute Emulator (formerly known as Dev Fabric). Windows Azure Storage Emulator has to be running.

After we introduce support for non-Azure persistent queuing, you will be able to develop and run applications locally, without Azure SDK, but still deploy then to the Windows Azure cloud afterwards.

**You can already run Lokad.CQRS Application Engine (and systems built with it) locally without Windows Azure SDK.  However this requires use of in-memory queues, which are not persistent across the system restarts.**