Snippet: Configurable PubSub Router
===================================

> This code is supposed to be used in small to medium projects in the 
> beginning of their lifecycle. As project matures it is recommended 
> to switch to something more robust like AMQP server for the routing 
> and pub/sub.

This router does 3 things:

1. Maintains routing (publish/subscribe) map
2. Routes event to subscribers using this map
3. It routes commands to a single queue (hardcoded)


This snippet could be expanded to support command partitioning (entity 
partitioning) or topic-based subscriptions in a similar manner.

Routing map is persisted in the atomic storage (using whatever storage system
is configured for it). Routing map can be managed remotely via the control
messages which come in the form of envelopes without the content but with
special attributes:

**To subscribe**:

key: router-subscribe-QUEUE-NAME
value: REGEX

**To unsubscribe**:

key router-unsubscribe-QUEUE-NAME
value: REGEX

Where REGEX matches against the full name of the message event type. In your
scenario match could be implemented differently. 

See the code for more details and comments. Unit test shows some sample usage