HttpEndpoint
============

> Added by Rinat Abdullin on 2011-08-25
> Extended by Alexandr Zaozersky on 2011-09-01

This snippet shows how to plug in and fire up a simple non-blocking Http server
that is available as Lokad.Cqrs extension. 

You can this use Http Server to:

* expose http endpoint for securely pushing commands from client to Cqrs 
  server (this includes pushing to memory queues and file queues over network)
* expose http endpoint for querying read models (which could reside in memory)
* perform custom filtering of large read models on the server (aka large lists)
* expose endpoint for accessing event streams securely

How to check out?
-----------------

* Start the test (in Using.cs)
* Go to http://localhost:8082/index.htm in your browser
* Start dragging lokad-cqrs logo around
* Check out source code for comments and details.
* Kill the engine and try dragging the image around.


Note, that this snippet requires either running as admin or adding port 
reservation. Unit test will print out details, if fails.

    netsh http add urlacl url=http://+:8082/ user=RINAT-PC\Rinat.Abdullin

