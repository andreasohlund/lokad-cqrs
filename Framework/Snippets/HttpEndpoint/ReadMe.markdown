HttpEndpoint
============

> Added by Rinat Abdullin on 2011-08-25

This snippet shows how to plug in and fire up a simple non-blocking Http server
that is available as Lokad.Cqrs extension. Please refer to Lokad.Cqrs.Http docs
for more details.

You can this Http Server to:

* expose http endpoint for securely pushing commands from client to Cqrs 
  server (this includes pushing to memory queues and file queues over network)
* expose http endpoint for querying read models (which could reside in memory)
* perform custom filtering of large read models on the server (aka large lists)
* expose endpoint for accessing event streams securely



