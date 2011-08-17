---
layout: post
title: Chapter 07 - Basics of Lokad.CQRS Application Engine and Client
---

# Basics of Lokad.CQRS Application Engine and Client
Lokad.CQRS framework provides a set of reusable software blocks that build upon the theoretical principles outlined in the previous chapters. These blocks help to design and build distributed systems, while guiding around some of the problems and technicalities that were discovered and experienced by Lokad teams.

Lokad.CQRS provides configurable .NET Application Engine. This is a light-weight stand-alone server that could be hosted in a console process, cloud worker role or even a unit test. This Engine hosts the following functionality:

* Managing message contracts, serialization formats and transport envelopes.
* Sending messages to supported queues:
	
    * In-Memory
    * Azure Queues
* Long-running task processes that receive messages from queues and dispatch them to the available message handlers.
* Message scheduling for the delayed delivery of the messages to the recipients.
* Message routing for implementing load balancing and partitioning.
* Dependency Injection features for exposing additional services to the message handlers.
* Support for the Storage Blocks.
* Special builder syntax for configuring all this functionality.
* Numerous extensibility points for providing custom behaviors or introducing new features.

In addition to that, there is a **.NET Application Client**, offering subset of features required to build User Interfaces and APIs. This Client is a set of helper classes that are meant to be reused in an application and also feature configuration syntax.

## Configuring and Running Lokad.CQRS Application Engine
Before the engine could be started, it has to be configured via the builder syntax. This configuration happens in the code and sets up the tasks that will be executed by the server, while it is running.

Some of these tasks are responsible for routing, receiving and dispatching messages; so the configuration has to set up proper message contracts and serialization rules.

Storage Blocks, that were described earlier, could also be configured here. They will be made available to the message handlers via Dependency Injection, just like the rest of the components.

In addition to that, builder syntax exposes **optional hooks and extensibility points**, which could be used to modify and enhance certain parts of the system with custom behaviors. There are 3 types of the extensibility points available from within the configuration:

* Providing specific behaviors by supplying custom interface implementations, where builder syntax accepts them.
* Subscribing to system events and reacting to them with the help of Reactive Extensibility framework for .NET.
* Injecting custom components and implementations directly into the IoC Container used by the Application Engine.

To simplify development and initial introduction, Application Engine is designed to self-configure itself with the sensible defaults, where appropriate. It will also try to orchestrate tasks and adjust some settings to achieve the best performance. Obviously, you can override and customize all this behavior.

Some of the defaults include:

* Using `DataContractSerializer` (native to the .NET Framework) to persist messages.
* Locating message contracts by scanning all loaded assemblies for classes deriving from `Define.Command` and `Define.Event`.
* Locating message handlers by scanning all loaded assemblies for classes deriving from `Define.Handle<TCommand>` (for command handlers) and for classes deriving from `Define.Consume<TEvent>` (for event handlers).
* Using sensible timeout strategies for polling Azure queues, to reduce the operational costs, while still keeping system responsive.

Here's a sample snippet of the basic configuration for the engine to run locally, using in-memory atomic storage and in-memory partition:

	var builder = new CqrsEngineBuilder();
	builder.Storage(m => m.AtomicIsInMemory());
	builder.Memory(m =>
		{
			m.AddMemoryProcess("in");
			m.AddMemorySender("in");
		});

If we want to run the same code under Windows Azure using ProtoBuf for the serialization:

	var account = AzureStorage.CreateConfig(storageAccountString);
	var builder = new CqrsEngineBuilder();
	builder.Storage(m => m.AtomicIsInAzure(account));
	builder.Azure(m =>
		{
			m.AddAzureProcess(account, "in");
			m.AddAzureSender(account, "in");
		});
	builder.UseProtoBufSerialization();

Note, that `AddAzureSender/AddMemorySender` are responsible for configuring the default instance of IMessageSender that will be available to components (i.e.: message handlers) via the dependency injection. You need to configure only one sender, defining the default queue to which it will send messages.

**Check unit tests in Lokad.CQRS sources for the samples of configuring Application Engine and running it in memory.**

This document section merely provides a high-level overview of the configuration process. Specific configuration options will be discussed later.

In order to run Lokad.CQRS application engine, given completed builder we can do something like this:

	try
	{
		using (var token = new CancellationTokenSource())
		using (var engine = builder.Build())
		{
			engine.Start(token.Token);

			Console.WriteLine("Press any key to stop.");
			Console.ReadKey(true);

			token.Cancel();
			if (!token.Token.WaitHandle.WaitOne(5000))
			{
				Console.WriteLine("Terminating");
			}
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex.Message);
		Console.ReadKey(true);
	}

In this snippet we create an engine instance out of the builder and start it using cancellation token from the Task Parallel Library of .NET 4.0. The latter allows us to gracefully shutdown all engine services later, with an opportunity to terminate them, if shutdown takes too much time.

## Configuring and Using Lokad.CQRS Application Client
Lokad.CQRS Application Client is merely a set of helper classes that could be configured via builder syntax. The latter resembles builder syntax of the Application Engine, but is more limited in features. The biggest difference is that Application Client does not host any long-running processes and is not capable of receiving messages.

Here's a sample of configuration snippet for the Client that is capable of sending message to Azure queues and accessing persistent read models (Views) stored within the Azure Storage Account:

	var account = AzureStorage.CreateConfig(storageAccountString);
	var builder = new CqrsClientBuilder();
	builder.Azure(c => c.AddAzureSender(account, "in"));
	builder.Storage(s =>
		{
			s.AtomicIsInAzure(account);
			s.StreamingIsInAzure(account);
		});
	var client = builder.Build()

Given this code, we can access various services provided by the client:

	// read a view
	var storage = client.Resolve<NuclearStorage>();
	var singleton = storage.GetSingletonOrNew<UserDashboardView>();

	// send a message
	var sender = client.Resolve<IMessageSender>();
	sender.SendOne(myMessage);

	


## Message Directory
Lokad.CQRS needs to know how to find message contracts and to wire them into message handlers. If you are using the default approach, it will do everything automatically.

Default approach is:

* Derive messages from the interfaces `Define.Event` or `Define.Command`.
* Derive message handlers from the interfaces `Define.Handler<TCommand>` and `Define.Consumer<TEvent>`.

Just like with the atomic storage contracts, we are using default Define interfaces. They are recommended to be used only for getting started with Lokad.CQRS in you project, and thus they are grouped under the same class for better visibility. 

Please keep in mind, that this simple approach creates a dependency between Lokad.CQRS assemblies and your libraries for message contracts and message handlers. This is something that is recommended to be avoided in complex projects, since it tends to increase the risk of dependency hell: various build and upgrade problems due to the version conflicts. A bit later we will discuss a way to decouple contracts and handlers from Lokad.CQRS assemblies, showing some configuration samples.

That's how message contracts are defined via the default simplified approach using data contract serializer for this sample:

	[DataContract]
	public sealed class GoMessage : Define.Command { /* members */ }
	[DataContract]
	public sealed class FinishMessage : Define.Command{ /* members */ }

And the consumer:

	public sealed class GoHandler : Define.Handle<GoMessage>
	{
		readonly IMessageSender _sender;
		readonly NuclearStorage _storage;

		public GoHandler(IMessageSender sender, NuclearStorage storage)
		{
			_sender = sender;
			_storage = storage;
		}
		public void Consume(GoMessage message)
		{
			var result = _storage.UpdateSingletonEnforcingNew<Entity>(s => s.Count += 1);

			if (result.Count == 5)
			{
				_sender.SendOne(new FinishMessage());
			}
			else
			{
				_sender.SendOne(new GoMessage());
			}
		}
	}



## Using Storage Blocks
Builder syntax for both Lokad.CQRS Application Engine and Client has shortcuts to configure Storage Blocks into the internal IoC Container. Doing so will expose associated interfaces to message handlers and other components that might be requesting them via constructor injection.

For the Atomic Storage, you can use one of these options (with numerous overloads):

	builder.Storage(m => m.AtomicIsInFiles("path-to-my-folder"));
	builder.Storage(m => m.AtomicIsInAzure(myAzureAccount));
	builder.Storage(m => m.AtomicIsInMemory());

You can also provide additional atomic storage configuration options or even a custom atomic factory implementation, which will get wired to the container.

Here's a sample of plugging in atomic storage with some non-default settings for view contract lookup and `JsonSerializer` from `ServiceStack.Text`:

	builder.Storage(s =>
		{
			s.AtomicIsInAzure(data, b =>
				{
					b.CustomSerializer(
					  JsonSerializer.SerializeToStream, 
					  JsonSerializer.DeserializeFromStream);
					b.WhereEntity(t => 
					  t.Name.EndsWith("View") && 
					  t.IsDefined(typeof(SerializableAttribute),false));
				});
		});

Streaming storage is also supported with the builder syntax. You can use one of these options to configure it:

	builder.Storage(m => m.StreamingIsInFiles("path-to-my-folder"));
	builder.Storage(m => m.StreamingIsInAzure(myAzureAccount));


## Message Queues
Lokad.CQRS Application engine can process messages coming from two different types of message queues:

* In-memory message queue - configured by `builder.AddMemoryProcess`.
* Windows Azure Cloud Queues - configured by `builder.AddAzureProcess`.

There are plans to add simple file-based queues and AMQP support.

Each queue implementation supports:

* Volatile transactions (`System.Transactions` namespace).
* Automatic message deduplication handled by bus.
* Future messages - messages that are delivered upon the specified time, i.e. delayed messages.



### Azure Queues
Azure Queues implementation is built on top of Azure Storage Cloud Queues and provides native support for a variety of scenarios. It features:

* Automatic handling of message envelopes larger than 6144 bytes (or 8192 bytes in Base64 Encoding).
* Queue polling scheduler that reduces number of checks, when application sits idle (to reduce Azure charges), which is configured by DecayPolicy.
* Message delivery scheduler that allows delaying message delivery till certain point in time.


### In-Memory Queues
In-memory queues are designed to be extremely simple, non-persistent and fast-performing. They are used in high frequency aggregation operations (where losing some messages is not an issue), internal unit tests or when firing up Lokad.CQRS engine inside another application for various support and helper uses.

In-memory queues support message delivery scheduling as well.

## Message Dispatchers
Message dispatchers in Lokad.CQRS Application Engine are the "glue" that links together message envelopes and code to handle them. It is one of the most important extensibility points in the Application Engine.

Different message handling scenarios might require different dispatch behaviors. Lokad.CQRS project includes pre-built dispatcher implementations required to develop a system following the Decide-Act-Report model (DAR). 

Each of these dispatcher implementations could be configured in the builder syntax.

There are plans to add fourth type of the dispatcher, mapping message envelopes directly to the Aggregate Root entities with Event Sourcing.

### Command Dispatcher
Command batch dispatcher dispatches incoming message envelope (with one or more command messages) against the handlers in a single logical transaction. It enforces the rule that each message could have only one handler.

This dispatcher is used for configuring server-side domain partitions ("Act" element of DAR):


	builder.AddAzureProcess(dev, IdFor.Commands, x =>
		 {
			 x.DirectoryFilter(f => f.WhereMessagesAre<IDomainCommand>());
			 x.DispatchAsCommandBatch();         
		 });


`DispatchAsCommandBatch` is a builder shortcut for wiring the same dispatcher manually. 


### Event Dispatcher
Event dispatcher dispatches incoming message envelope (with exactly one message item) against available handlers in a single logical transactions. There could be zero or more handlers.

This dispatcher is used to configure processes handling events (for example, view model denormalizers - "Report" element of DAR):

	m.AddAzureProcess(dev, IdFor.Events, x =>
		{
			x.DirectoryFilter(f => f.WhereMessagesAre<IDomainEvent>());
			x.DispatchAsEvents();
		});

`DispatchAsEvents` is builder shortcut for wiring this dispatcher.

### Routing Dispatcher
Lokad.CQRS has a default implementation of a simple rule-based router. This router could be used for distributing the load between multiple processes or partitioning a distributed system. 

Configuration might look like this for the memory process:

	builder.AddMemoryRouter("in", 
	me => (((Message1) me.Items[0].Content).Block%2) == 0 ? "memory:do1" : "memory:do2");

Please, note the syntax of the routing rule, which is actually defined as `Func<ImmutableEnvelope, string>` where the returned string should be in form of **queue-factory-name:queue-name**.

Memory queue factory is registered automatically with the prefix "memory". 

Additional queue factories are automatically registered in the container, whenever you add a message sender.  Alternatively you can register your custom factory manually:

	builder.Advanced.RegisterQueueWriterFactory(c => 
		new AzureQueueWriterFactory(config, c.Resolve<IEnvelopeStreamer>())
	);

If you are adding Azure sender, then the registered factory prefix will match to the Azure storage account name, unless you specify a custom one:

	var account = AzureStorage.CreateConfig("myString", => c.Named("azure-dev"));