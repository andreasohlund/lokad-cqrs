---
layout: post
title: Chapter 03 - Overview
---

# Overview
Lokad.CQRS is a .NET framework and guidance for designing and building distributed systems that target cloud computing environments (like Windows Azure). Framework is written in C# but could be used in any other .NET language.

Lokad.CQRS is designed to be non-intrusive, while guiding development away from some common pitfalls and problems that we've encountered while building scalable systems for the cloud.
This project is driven by our desire to push further state of the art in the field and inspired by a set of principles:

* **Messaging** - way of decoupling systems by encapsulating method calls into message objects that could be sent over the networks, queued or persisted.
* **Domain-Driven Design** - practices for design and modeling complex business domains in software.
* **Command-Query Responsibility Segregation (CQRS)** - separation between read/write responsibilities at the class level and within the architecture.
* **Decide-Act-Report** pattern - way of modeling and designing client-driven message-based applications with the use of messaging, intent-capturing UI, decoupled "write side" and persistent read models.
* And many more.

Experience at Lokad suggests that these principles help building systems that can scale and grow in complexity as needed, while still requiring limited development resources for further evolution and maintenance. 

In addition to that, Lokad.CQRS explicitly forces you to stick diligently to the most important principles. It is designed this way. In return it grants limited portability - ability to run the same system under various environments with minor changes:

* Local development environments;
* On-premise production deployments;
* Cloud-computing deployments;
* Various combinations of these items.



## Crash Course
From the simplest perspective, distributed systems are composed from messages, which are transported between various components hosted somewhere (on a server, in the cloud or locally). These logical components receive messages via message handlers, which act upon them (i.e. change database or other data) and optionally send some more messages. 

**Decide-Act-Report** model explains how to build feature-rich applications out of these elements, while **Domain-Driven Design** helps to tackle the complexity and model the business domain.

Lokad.CQRS provides help in writing these messages, message handlers, dealing with the data, configuring everything together with a set of technologies, and then running everything on a remote server, locally or in the cloud.

Go to <http://abdullin.com/cqrs> to start learning more about CQRS and distributed systems. 

Check out Samples within the project for some practical guidance on building system with Lokad.CQRS using:

* Microsoft .NET Stack
* ASP.NET MVC


## Project Structure
Lokad.CQRS consists of the following important elements:

* Storage Blocks (in Lokad.CQRS.Portable.dll)
* Application Engine and Client with builders (in Lokad.CQRS.Portable.dll)
* Windows Azure Extension Package (in Lokad.CQRS.Azure.dll)
* Samples
* Documentation and Guidance

Let's go over these elements.

![] (https://github.com/Lokad/lokad.github.com/raw/master/cqrs/chapter-03-content/figure-03-00.png)
