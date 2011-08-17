---
layout: post
title: Chapter 06 - Storage Blocks
---

# Storage Blocks
Lokad publishes Lokad.CQRS as an open source to share a few reusable blocks for building distributed applications. These blocks have significant potential for using as stand-alone components and also as a part of Lokad.CQRS Application Engine (to be discussed later).
Some of the most important blocks in the project deal with storage and persistence from the NoSQL perspective. 

## Why NoSQL?
Traditionally for the persistence in .NET world SQL databases were used. They are based on the mathematically-proven concepts of [relational model] (http://en.wikipedia.org/wiki/Relational_model) and [relational algebra] (http://en.wikipedia.org/wiki/Relational_algebra). SQL databases offer a wide range of advanced features and are considered to be suitable for a wide range of application scenarios. They still seem to be the most prominent choice in the industry for implementing persistence in .NET systems.

However, SQL databases (and relational approach in its own) may not be the most simple and fast route for building distributed systems for the cloud. This comes from the fact that distributed environments (as opposed to on-premises systems operating on a single machine or a tight data-center) introduce a whole range of new factors into the picture:

* High latency and eventual consistency of data.
* Higher probabilities of various network, storage and node failures.
* Higher deployment and maintenance complexities.
* Requirement to support almost-infinite scalability scenarios (including elastic scaling).

Although SQL databases have a long history of solving these problems (and have developed highly sophisticated set of tools and features for that), there could be a simpler approach. It requires making a step from the relational model back to NoSQL and handling some cases explicitly.

At Lokad we believe that this approach allows developing cloud systems faster and with better efficiency. Highly opinionated storage building blocks of Lokad.CQRS are based on some of our successes and help to share this experience.

Each storage block consists from a set of interfaces that model interactions with some specific type of NoSQL storage, while guiding developers to build better code. There also are various implementations of these interfaces for each type of the storage. This separation allows coding the system once and then be capable of switching between different storage implementations. For example, you can run same component against in-memory storage for unit tests, on top of file system for local development and against cloud storage for the production. 

## Atomic Storage
Atomic storage is a set of interfaces (and implementations) modeled after the concurrency primitives and designed for safe state persistence in distributed and cloud environments where:

* Noticeable latency is expected (i.e. inserting 100000 records might take minutes).
* Entities are relatively small (i.e. generally under 10MB in size).
* There could be multiple writers and readers (high concurrency).
* Storage engine supports atomicity (i.e.: via locking or tags).

Essentially, Atomic Storage is just about persisting and retrieving some documents. In our code we don't care, how exactly the documents are serialized or persisted (this will depend on the configuration). But we do care about writing scalable and distributable code. Most common use of this storage type in the CQRS world is for Views (a la Persistent Read Models).

The most important idea behind the atomic storage is **implicit atomicity of the write operations**. Consider this simplified scenario, where 2 distinct processes try to concurrently add various bonuses to the customer document:

1.	Process A reads Customer-3. It has 4 bonus points.
2.	Process B reads Customer-3. It has 4 bonus points.
3.	Process A adds 2 bonus points to customer (total of 6) and saves Customer-3.
4.	Process B adds 1 bonus point to its retrieved copy (total of 5) and saves Customer-3.

The result is that customer has only 5 bonus points, while it should've had 7 (4+2+1). One way to work around this concurrency problem is to use various explicit locks. If, however, you are writing a lot of such code, managing locks can be tedious.

In Atomic Storage, all operations modifying documents (i.e.: Update, AddOrUpdate, TryDelete) have such method signatures that we can write concurrency-safe code without even bothering about locks, leases, tags or any other mechanisms for handling this sort of problems. Implementations of the storage interfaces can use their own preferred locking mechanism for preventing (and optionally even automatically handling) race conditions.

Document contracts for this storage are defined in the code as serializable classes:

	[DataContract]
	public sealed class Customer : Define.AtomicEntity
	{
		[DataMember] public string Name;
		[DataMember] public string Address;
	}

Here we are using default serialization attributes (`DataContract` and `DataMember`), which work for the `DataContractSerializer`. The latter is the default serializer used by Lokad.CQRS. However, you certainly can use different serialization format for persisting your entities.

Note, that we are using the default `Define.AtomicEntity` interface to mark our documents. This is the default approach, which allows starting to work faster, but also creates dependency between Lokad.CQRS project and your own code. The latter is not recommended for systems with complex dependencies between the projects, since it makes harder to manage them and avoid version collisions (sometimes called as DLL hell). By providing your own implementation of atomic strategy you can decouple document contracts from Lokad.CQRS libraries.

### Entities vs. Singletons
There are two different types of documents supported and managed by the Atomic storage. 

**EntityDocument** is a document that has a unique identity (key), which could be used to locate and manage it. Customer is a sample of an entity; it might be identified by a customer ID.

**Singleton Document** is a document without an identity. Only instance of this document can exist within a system (or sub-system). System configuration is a sample of a singleton document. It's shared across the system.

The difference seems to be really subtle, yet managing these types of documents internally could be completely different story. For example, entities are scaled by partitioning, while singletons must be locked and replicated.

To enforce this difference there are two separate sets of interfaces and methods within the Atomic storage: dealing with singletons and managing entities. 

### NuclearStorage vs. Typed Interfaces
Functionality behind the Atomic Storage could be leveraged via two ways.

Typed Interfaces are the default and recommended way, when you are working with the persistent read models (views). This way involves requesting strongly-typed document reader/writer interface instances from the factory or IoC Container. Such instance will be capable of working with a single document type only.

This provides compile-time checks, more compact code and maintenance support for managing persistent read models. Below is a code snippet of a view handler for `ReportDownloadView` that requests strongly-typed atomic storage interface `IAtomicEntityWriter<Guid, ReportDownloadView>` from the constructor and uses it to manage the view.

The first parameter in the interface is the type of the key, while the second one - type of the view itself.
	
	public sealed class ReportDownloadHandler :
		ConsumerOf<ReportCreatedEvent>,
		ConsumerOf<ReportDeletedEvent>
	{
		readonly IAtomicEntityWriter<Guid, ReportDownloadView> _writer;

		public ReportDownloadHandler(IAtomicEntityWriter<Guid, ReportDownloadView> writer)
		{
			_writer = writer;
		}

		public void Consume(ReportDeletedEvent message)
		{
			_writer.TryDelete(message.ReportId);
		}

		public void Consume(ReportCreatedEvent message)
		{
			var view = new ReportDownloadView
				{
					StorageReference = message.StorageReference,
					StorageContainer = message.StorageContainer,
					ContentType = message.ContentType,
					FileName = message.FileName,
					AccountId = message.AccountId
				};
			_writer.AddOrUpdate(message.ReportId, view, x => { });
		}
	}

However, in certain scenarios this strongly-typed approach might lead to repetitive and excessive code. Hence, we also provide an alternative route of using **NuclearStorage**. This is a helper class, that could be requested from the IoC container or created with a factory. It allows working with any type of the document at the expense of losing some compiler-time checks and maintenance capabilities. 
Here's an example of using **NuclearStorage** in the code:

	NuclearStorage storage = ...;
	var customer = new Customer()
		{
			Name = "My first customer",
			Address = "The address"
		};
	storage.AddEntity("cust-123", customer);
	storage.UpdateEntity<Customer>("cust-123", c =>
		{
			c.Address = "Customer Moved";
		});

	var result = storage.GetEntity<Customer>("cust-123").Value;
	Console.WriteLine("{0}: {1}", result.Name, result.Address);

	
### Readers vs. Writers
Lokad.CQRS explicitly enforces separation of reads from writes, where it makes sense. **Typed Interfaces** (used mainly for managing persistent read models) represent one example of that. We have reader interfaces that return and query the storage, and we have writer interfaces

This separation also allows us to implement some simple but clever optimizations within the implementations of the typed interfaces. 

### Configuration
You can currently configure AtomicStorage to work with the following stand-alone scenarios:

* File System - via FileStorage.CreateAtomic
* In-Memory - via MemoryStorage.CreateAtomic
* Windows Azure Blob Storage - via AzureStorage.CreateAtomic


## Streaming Storage
Streaming storage is another set of persistence interfaces and implementations for them. Streaming storage is designed to handle relatively large (i.e. in GBs) binary files stored locally or in the cloud. This is a highly specific scenario.

Following features are supported at the interface level:

* Compression and hashing logic
* Efficient copying and streaming
* Modified/ETag headers and storage conditions

Streaming storage targets highly specific scenario of large binary files (that potentially do not fit into the memory of a working process) and is a lower level of abstraction than atomic storage. Streaming storage **does not provide implicit atomic update operations**; developer is responsible for managing locks and concurrency issues. It also does not provide a uniform handling of serialization inside the interface; developer will need to **work directly with the streams**.

In order to work with this storage, you need to get an instance of `IStreamingRoot` either from the factory or from the IoC Container.

For example, here's how you can write a stream:

	IStreamingRoot root = AzureStorage.CreateStreaming(myAccount);
	var container = root.GetContainer("test1");
	container.Create();
	container.GetItem("data").Write(s =>
		{
			using (var writer = new StreamWriter(s,Encoding.UTF8))
			{
				writer.Write(new string('*',50000));
			}
		});
Here's an example of reading:

	string result = null;
	root.GetContainer("test1").GetItem("data").ReadInto((props, stream) =>
			 {
				 using (var reader = new StreamReader(stream, Encoding.UTF8))
				 {
					 result = reader.ReadToEnd();
				 }
			 });
	 Assert.AreEqual(new string('*', 50000), result);
  
  
  
### Optimizations
Streaming storage allows optimizing performance of certain operations, if the underlying storage supports the needed functionality. Compression optimization is done by using `StorageWriteHints` as one of the parameters. Currently only one hint is supported:

* **CompressIfPossible** - invokes GZip compression, if this is supported by the underlying storage. Decompression is handled automatically as well.

Another optimization is used for copying large streams and is handled transparently. For example, if we are copying large blob within the same Azure storage account, then this could be accomplished by a single Azure-native call. At the same time copying stream from Azure blob to a local file, will require full streaming.

### Streaming Conditions
Streaming conditions are a way to handle ETags and modifications dates rules of the underlying storage. They are used to implement efficient caching (i.e.: there is no need to download a large stream, if it has not changed) and change locking (i.e.: we instruct storage engine to fail if we try to overwrite a stream, that has changes since we've read it for the last time).

While reading streams or querying item information, you gain access to the `StreamingItemInfo` which contains ETag and LastModifiedUTC information. This information could be used while writing to a stream or copying it. Streaming conditions are composed via `StreamingCondition` class. 
Following conditions are supported for the tags (with wild cards):

* IfMatch
* IfNoneMatch

Following conditions are supported for the modification date:

* IfModifiedSince
* IfUnmodifiedSince



### Configuration
You can currently configure Streaming Storage to work with the following stand-alone scenarios which return an instance of `IStreamingRoot`:

* FileStorage.CreateStreaming
* AzureStorage.CreateStreaming