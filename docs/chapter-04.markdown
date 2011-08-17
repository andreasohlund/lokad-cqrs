---
layout: post
title: Chapter 04 - Core Development Concepts
---


# Core Development Concepts

There are certain development concepts that are essential to Lokad.CQRS, understanding it properly and successfully delivering software. These concepts will be briefly mentioned in this chapter just to ensure that we are on the same page regarding the terminology and application scenarios.
Please, feel free to skim through this chapter, if the concepts are already familiar to you.

## Inversion of Control
**Inversion of Control (IoC)** is an approach in software development that favors removing sealed dependencies between classes in order to make code more simple and flexible. We push control of dependency creation outside of the class, while making this dependency explicit.

Usage of Inversion of Control generally allows creating applications that are more flexible, unit-testable, simple and maintainable in the long run.

Often control over the dependency creation is delegated to specialized application blocks called Inversion of Control Containers. IoC containers are really good in determining dependencies that are needed to create a specific class and injecting them automatically. This process is often called **Dependency Injection**.

[Continue reading...] (http://abdullin.com/wiki/inversion-of-control-ioc.html)

## Unit Testing
**Unit Testing** in software development is a way to quickly verify that smallest blocks of software (units) behave as expected even as the software changes and evolves.

Any program could be logically separated into distinct units (in object-oriented programming the smallest unit usually being a class). Developers, while coding these program units, also create tests for them (code blocks containing some assertions and expectations about units). These tests could be used to rapidly verify behavior of the code being tested.

When some other developer introduces new units or changes something in existing units, he can run all the tests available for the program and verify that everything is still operating as expected. Usually running unit tests is a fast operation (less than 30 seconds), so developers are encouraged to do that often.

[Continue reading...] (http://abdullin.com/wiki/unit-testing.html)

## Command-Query Responsibility Segregation
**Command-Query Responsibility Segregation (CQRS)** is a way of designing and developing scalable and robust enterprise solutions with rich business value.

In an oversimplified manner, CQRS separates commands (that change the data) from the queries (that read the data). This simple decision brings along a few changes to the classical architecture with service layers along with some positive side effects and opportunities. Instead of the RPC and Request-Reply communications, messaging and Publish-Subscribe are used.

If we go deeper, Command-query Responsibility Separation is about development principles, patterns and the guidance to build enterprise solution architecture on top of them.

[Continue reading...] (http://abdullin.com/cqrs/)

## Domain-Driven Design
**Domain-Driven Design (DDD)** is a way of understanding, explaining and evolving domain model in software in such manner that:

* model would focus on the most important characteristics of the problem at hand (while putting less important things aside, for the sake of preserving the sanity of everybody);
* the model could evolve and still stay in sync with reality;
* model would help different people with various backgrounds to work together (i.e.: users, sales people and hard-core developers);
* model would let you avoid costly development mistakes (it could even help to deliver new exciting features as a simple logical extension of what has already been implemented).
Domain-Driven Design is also a way of thinking, learning and talking about the business problem in a manner that implementing everything would be rather simple, despite the initial complexity of the actual problem.

[Continue reading...] (http://abdullin.com/journal/2010/11/16/key-cqrs-ingredient.html)

## Cloud Computing
**Cloud computing** is all about hardware-based services (involving computing, network and storage capacities), where:

* Services are **provided on-demand**; customers can pay for them as they go, without the need to invest into a datacenter.
* Hardware management is abstracted from the customers.
* Infrastructure capacities are elastic and can easily scale up and down.
There is a powerful economic force behind this simple model: providing and consuming cloud computing services generally allows having far more efficient resource utilization, compared to self-hosting and data center type of hosting.

[Continue reading...] (http://abdullin.com/wiki/cloud-computing.html)
