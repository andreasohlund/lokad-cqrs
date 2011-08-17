---
layout: post
title: Chapter 05 - Messaging and Distributed Systems
---

# Messaging and Distributed Systems
Messages are essential to building robust and distributed systems, so let's talk a bit about them.

**Message is a named data structure**, which can be sent from one distributed component to another. Components can be located on the same machine or on different sides of the Earth.  

The basic example of message is an email. It has a sender, subject and one or more recipients. This email might take some time to reach the designation. Then it gets to the inbox, where it could spend even more time, before recipient finally gets time to read it and may be even reply.

Messages, just like emails, might take some time to reach recipient (it could be really fast but it is not instantaneous), and they could spend some time in the message queues (analogue of inbox), before the receiving side finally manages to get to work on this message.

The primary problem with messages and emails is their asynchronous nature. When we send email or message, we expect the answer some time later, but we can never be sure that we will get it right away. Direct phone calls (or direct method calls) are much better here - once you get the person on the phone, you can talk to him in real time and get results back almost immediately.

Despite these asynchronous nature problems, messages could be better than calls for building distributed and scalable systems.

With phone calls and method calls you:

* Can get response immediately, once your call is picked up.
* Must be calling, while the other side is available (greater distance you have, harder it is to negotiate the call).
* More stressed the other side is, more time it will take before your call will be picked up. And this does not guarantee, that you will get the answer (when the other side is really stressed you are likely to get: we are busy right now, call next millennia).

With messages you:

* Can send a message and then get back to your work immediately.
* Must organize your work in such a way, that you will not just sit idle waiting for the response.
* Can send a message any time, the other side will receive and respond, as soon as it gets to the job.
* More stressed the other side is, more time it takes to receive the answer. No matter what the level of stress is, the other side will still be processing messages at its own pace without any real stress.

Since we are mostly interested in building distributed and scalable systems (which can handle stress and delays) messages are a better fit for us, than the direct method calls in the majority of the cases. They allow decoupling systems and evenly distributing the load. Besides, it is easy to do with messages such things like: replaying failing messages, saving them for audit purposes, redirecting or balancing between multiple recipients.

Note, that there are cases, where direct calls work better than messaging. For example, querying in-memory cache does not make sense with messaging. Cache is fast and you want to have the response immediately.

## Message Envelopes and Serialization
Message delivery is handled by the infrastructure and messaging middleware. In order to accomplish that task, additional information has to be associated with that message, while it travels from the sender to recipient. This additional information is called **out-of-band data** and is usually represented as transport headers sent with the messages.

Common samples of transport headers are:

* Message identity - unique identifier of the message, used to detect and discard potential duplicates.
* Message delivery date - time-stamp used to delay delivery of the message, till some time.
* Message routing key or topic - fields used to control pub/sub and load balancing in the messaging infrastructure.
* Sender's vector clock - used to implement [partial message ordering] (http://en.wikipedia.org/wiki/Vector_clock) in volatile environments.
* Contract information - name of the serialization format and contract, which could be used to build message object from the binary data.
* Signature - to verify authenticity of the message.

While sending message data across the network or saving it into some storage, all this information is packed along with the data into a structure called message envelope.

**Message envelope** is an atomic unit of transportation for messages, which includes the actual message data and all the information needed to reach the recipients and be processed properly. Envelopes are expected to be transparently handled by the infrastructure, while the actual application code deals with the message data that is already deserialized into objects.

This brings us to another important topic - message serialization, contracts and their versioning. 

**Message serialization** is the process of a way to transforming message object into a format that can be safely persisted or sent into the wire. **Message deserialization** is the reverse process of rebuilding object from the data.

In the .NET world some of the common serialization formats are:

* Xml serialization - format object as XML document.
* Binary serialization - encode object as a binary blob.
* JSON serialization - format object into a [JavaScript Object Notation] (http://en.wikipedia.org/wiki/JSON).
* [Google Protocol Buffers] (http://code.google.com/apis/protocolbuffers/) - encode objects into a compact binary blob.

Given the following message class definition (**message contract**):

	public sealed class ForecastsAvailableEvent : IDomainEvent
	{
		public ForecastsAvailableEvent(long solutionId, Guid syncId)
		{
			SyncId = syncId;
			SolutionId = solutionId;
		}

		public long SolutionId { get; private set; }
		public Guid SyncId { get; private set; }
	}

This would be an example of creating new instance of message object and sending it:

	int solutionId = ...
	Guid syncId = ...
	var message = new ForecastsAvailableEvent(solutionId, syncId);
	sender.SendOne(message);

The messaging infrastructure will take care of serializing this object into some preconfigured data format, adding required transport headers and sending the entire enveloper to the recipient. 

If we were to use human-readable JSON serialization formats, then intercepting message envelope half-way, would reveal us something like this:

	EnvelopeId: 8f38f84b-d13a-4908-9693-49cb00c1968b
	Created: 2011-06-01 00:38:01 (UTC)

	0. Salescast.Context.Sync.ForecastsAvailableEvent
	{
	  "SolutionId": 426,
	  "SyncId": "10a6e5d8-49cd-4b5e-8cc6-9ef50000022c"
	}

The above data fragment contains all the information required for the recipient to create an instance of `ForecastsAvailableEvent` and hand it over to the application code. Of course, in order to do that in a strongly-typed fashion, the receiving side has to know about the message contract. 

**Message contract** contains specific instructions about serializing specific members of a message object to a data format and then deserializing it back. In the case of our sample, where we've used JSON serialization format, default message contract is derived from the class structure automatically. In some other cases you might need to specify contract with the use of attributes, like this example of class decorated for .NET Protocol Buffers serialization format:

	[ProtoContract]
	public sealed class ForecastsAvailableEvent : IDomainEvent
	{
		public ForecastsAvailableEvent(long solutionId, Guid syncId)
		{
			SyncId = syncId;
			SolutionId = solutionId;
		}

		[ProtoMember(1)] public long SolutionId { get; private set; }
		[ProtoMember(2)] public Guid SyncId { get; private set; }
	}

Managing contracts becomes increasingly important in the large distributed systems that are continuously being evolved. 

If we consider serialization format to be the language, then message contracts (and the meaning behind them) are the actual verbs of this language. If two sub-systems (that could be located on the same machine or across the globe) are to exchange messages, they need to know both the language (serialization format) and have the same vocabularies (message contracts).  Otherwise the dialogue would be impossible or flawed.

However, in any evolving system, things change rather frequently. In the software world message contracts are changed rather frequently. Contract names change; members are added, removed or renamed to match the intent better; new message types are added. This is similar to how every new human generation tends to invent new words and come up with the new meanings to the old ones.

In order to avoid any misunderstandings between different generations of the software, we must ensure that message contracts are managed explicitly. You can either define any new version of the message contract as a completely different message, or you can update the message contract itself on all sides.

Sometimes the serialization format itself helps to simplify the evolution. For example, if you rename a member (property or a field) in a XML-serialized message, then the receiving side will not be able to read it unless:

* It either gets the updated version of the message contract.
* Or message contract on the sender side is adjusted accordingly to keep the serialization convention as it is.

However, all serialization formats are not born equal when it comes to deal with changes. For example, one of the major advantages of the ProtoBuf serialization format is that it precisely facilitates the contract evolution.

## Decide-Act-Report Model
In the previous section we've maintained an analogy between messages - emails and method calls - phone calls. In this section we will maintain this analogy, while establishing a new one - between real-world organizations and distributed systems that could be built to support such organizations.
Let's think how some real-world organizations might function like. With some imagination you can identify 3 roles:

* Managers, that run organization; they read paper reports, decide and issue orders for the workers below them to execute.
* Workers, that receive orders, act upon them (where they can and have the resources) and notify various departments about the job done.
* Assistants, which gather together all these notifications, mails and memos into various reports, making them available to anybody, who has to make the decision.

Obviously, the entire iterative process of decide-act-report takes some time. It is not instantaneous, because humans are slow. However, this somehow seems to works in the real world. Companies seem to make right decisions that guide them through the ever-changing business world. They even manage to grow into large organizations (with more complex structures).

In short, this structure - works. Now, take a look at the image below.

![] (https://github.com/Lokad/lokad.github.com/raw/master/cqrs/chapter-05-content/figure-05-00.png) 

This image is a representation of distributed architecture model commonly associated with CQRS-inspired software designs. It is just overplayed with the Decide-Act-Report (DAR) analogy from the real world.

In the software world, users are the managers, who **decide**, what to do in the UI. They use the latest reports available to them in form of views (aka Read Models or Projections) in a way that makes it simple to make a decision. **User interface captures their intent** in the form of **command messages**, which are sent to server to order it to do some job.

Servers, then, **act** upon the **command messages** sent to them. Upon the job completion (or any other outcome), notifications are sent to all interested parties in form of event messages published via pub/sub.

**View event handlers** (Projection Hosts) receive these notifications, building **Views** to **report** their data to the user. They work even harder to keep these up-to-date, updating them upon every event message. Since these reports are kept up-to-date, any interested party can query and an immediate result, without the need to wait for the report to be computed.

Everything is rather straightforward, as you can see. At the same time, some of the analogies from the real world can still apply. For example:

* There could be multiple managers, operating the same organization at the same time == multiple users can work concurrently with an application.
* If there is too much work, you can hire some more workers == if there are too many commands, you can add more servers.
* Actual reports can be copied and distributed around the organization, just in case manager needs them right now == you can spread read models around the globe to keep them close to the client (or even keep them at the client).
* Manager, workers and reporting assistants could be in the same building or they could be spread across the continents, while exchanging mail between each other == distributed application with messaging can have all components as in a single process or it can spread them across the data centers.

So, again:

* User - looks at views, decides and issues command messages.
* Command handlers - receive commands, act upon them and publish notifications (event messages)
* View handlers - receive interesting notifications and update views, making them available to the interested parties per request.

By the way, the same Decide-Act-Report model also works rather well, when you are trying to model interactions within the aggregate root (with Event Sourcing) or within the multi-threaded responsive UI.

## Scalability
Once we have decoupled our system between Decide-Act-Report roles with the help of messaging, it becomes possible to scale it in multiple directions:

* Performance scalability - distributing the load between elements to handle higher performance and reliability requirements.
* Development scalability - ability to distribute the development processes between various teams.
* Complexity scalability - breaking down the system into a set of decoupled elements and keeping any occasional complexity inside these elements.

While speaking about performance scaling, we have 3 options.

![] (chapter-05-content/figure-05-01.png)
 
 
* Scaling out the client (Decide) by load balancing - distributing the load between multiple machines, that will still read from views and push command messages.
* Scaling out the domain logic (Act) by partitioning entities - putting different entities on different machines based on some hashing function from their identity.
* Scaling out the read side (Report) by replicating views to multiple storage engines (or even pushing some views to the Content Delivery Networks around the globe)

All this is made possible (and relatively easy to achieve) by the use of messaging to decouple systems. At the very extreme this leads to the possibility of building [almost-infinitely scalable systems] (http://abdullin.com/wiki/infinitely-scalable-system.html).

## Entity Partitioning
Scaling the performance of the system by replicating views and load balancing the client side - is relatively easy. There are no real recurring race conditions. Any race conditions that could happen on the client side are just business processes that have not been explicitly modeled in the domain.

However, you can't just copy the command handling (Act side) between multiple machines. This creates an opportunity for inconsistent data. Imagine that customer-5 data is located on machines A and B. Then a message comes from the client side, telling server to rename the customer-5. You would need to somehow update both machines, locking the data and handling potential transaction failures via the 3rd party distributed transaction coordinator. This is not good.

Another approach of scaling out the command handling is to simply avoid any race conditions and keep write data owned by a single process. In order to scale we will keep different data objects on different machines, routing messages to them by a certain rule. Whenever we need to add more processing and storage capacities - just add more machines into the mix.

Let's establish a few terms, before we proceed further.

**Entity** - is some model or data, which can be uniquely identified by a key, which is called an identity. Entity keeps the key for its lifetime. Identities are unique within the scope of the system. Two different entities can never share the same identity.

Identities can be natural (like customer SSN) or artificial (like integral ID or GUID).

**Partitioning entities** (the command side or Act side) is just about putting different entities to different partitions, based on some hashing or partitioning rule. Rule merely answers the question of "If entity's key is X, then it is stored on partition A". Rule can be as simple as: "take the last digit of the identity" or more complex, mapping some VIP entities (i.e.: customers with "Gold" status) to dedicated servers.

When command message is sent to an entity, it is automatically routed to the correct partition, based on this rule.
 
![] (chapter-05-content/figure-05-02.png)

Notion of partition is an important concept in building distributed systems, especially within cloud environments. Partition borders determine the outer limit for the bounded contexts and consistency boundaries. Transaction serializability is not supposed to cross them.

Although partition can contain multiple entities, these entities have to be considered as being in the separate partitions. It is never known in advance if after the next repartitioning they will still be neighbors.

For more information on the theory of building distributed systems, check out the following works of Pat Helland:

* [Life Beyond Distributed Transactions: An Apostate's Opinion] (http://www.ics.uci.edu/~cs223/papers/cidr07p15.pdf)
* [Memories, Guesses, and Apologies] (http://blogs.msdn.com/b/pathelland/archive/2007/05/15/memories-guesses-and-apologies.aspx)
* [Building on Quicksand] (http://arxiv.org/ftp/arxiv/papers/0909/0909.1788.pdf)
