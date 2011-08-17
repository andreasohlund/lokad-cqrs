---
layout: cqrs-case-study
title: Review 2011-07-11 - Human Record.com
---


# Human Record

This case study was kindly provided by [Tihomir Petkov](http://www.linkedin.com/in/tihomirpetkov), CTO and co-founder of [Human Record](http://www.humanrecord.com/). 

## 1. What is your business about? Why have you chosen to go distributed/cloud way?

Human Record is the marketplace for historic and vintage photos, illustrations, and fine art. We serve the international community of historic image professionals and passionate amateurs by providing an online platform for crowdsourced content aggregation and straightforward licensing. We decided to build Human Record in a distributed way because of two primary reasons. First, from an engineering perspective, we liked the reduced complexity and easier maintenance of a system based on loosely coupled autonomous components. This type of architecture also facilitates scaling, which is especially important for any web solution facing hard to predict workloads and traffic. And second, from a business point of view, having a distributed system running in the cloud has very tangible cashflow-related benefits: no upfront infrastructure expenses, greatly reduced administration overhead, and flexible ongoing infrastructure costs that can be easily scaled to fit the current demand.

## 2. How long have you been using Lokad.CQRS? How do you find it? 

We have been using Lokad.CQRS for about 11 months now and absolutely love it. We started playing with the pre-v1.0 framework, and then integrated it with our development codebase when the v1.0 was released. Our platform has been live for 5 months now and Lokad.CQRS has been a crucial component in the system. We were holding back upgrading to Windows Azure SDK 1.4 until Lokad.CQRS v2.0 was released, but we upgraded recently and we are now running the latest versions.

## 3. Why does Lokad.CQRS work for you? What is you favorite feature? 

Besides just working and taking away a great deal of complexity, Lokad.CQRS is an extensible and lightweight framework that incorporates best practices tested in the field by Lokad. The extensibility and lightweight parts are very important to us and together with the rest of the above features, Lokad.CQRS directly saves us development effort and costly, time-consuming errors.

Being open-source, Lokad.CQRS has also served us as an engineering guidance in our cloud development. Important corollary of the open source code is that we can fix bugs and tweak the core of the framework any time we need to, as well as contributing back to the community. This also means that we are not introducing a blackbox as an important system component which further reduces the complexity of debugging code related to the framework.

Our favorite Lokad.CQRS feature is message handling, which is crucial in distributed systems. The framework takes away most of the complexity of transporting and dispatching messages within a distributed system and makes communication between decoupled components much easier.

## 4. How many people do you have working on CQRS related projects? What's their take on getting started with Lokad.CQRS, on documents and community?

Currently we have 3 developers working on the platform. When starting  out with Lokad.CQRS, understandably there was a bit of a learning curve. I remember there was some confusion initially regarding the difference between the Lokad.Cloud and Lokad.CQRS projects, but Rinat's blog is always a helpful resource in such cases. The example projects are probably the fastest way for developers to get on track with the framework, but the ones on Google Code could be improved. For example, when we were upgrading to v2.0 not all example projects were updated to work with the latest code. I guess it's hard for you to dedicate your limited resources to documentation and examples but if framework adoption is among your priorities, improving the help resources would help a lot of people to get started. Rinat is always very active and helpful, so kudos for having him aboard. 

Our developers also shared that they like the new v2.0 documentation PDF that gives a nice overview of the framework, but it's not a good substitue for the more hands-on wiki-style documentation that used to be on the Google Code site.

## 5. What's next for you? What role does Lokad.CQRS play in these plans?

We will continue to develop our platform and expand both our userbase and portfolio. This will lead us to new challenges and opportunities, but Lokad.CQRS will remain an important foundation for our further growth.

## 6. Where can we find you online?

* Website: [http://www.humanrecord.com](http://www.humanrecord.com)
* Twitter: [http://twitter.com/Human_Record](http://twitter.com/Human_Record)
* Facebook: [http://www.facebook.com/humanrecordimages](http://www.facebook.com/humanrecordimages)
