---
layout: post
title: Chapter 02 - Project References and Cases
---


# Project References and Cases
Lokad.CQRS is used and battle-tested internally at Lokad in a number of projects targeting various business objectives, feature requirements and scalability challenges. 

Below is a quick overview of these projects.

## Salescast
Salescast is a cloud integration engine for massive inventory optimization. It is capable of processing millions of product references, tailored for large retail networks. 

Lokad.CQRS significantly simplifies the development while preserving cloud scalability. It allows achieving less than one hour interval between committing code and reliably deploying latest changes into production on Windows Azure.

![] (chapter-02-content/figure-02-00.png)

Features:

* Multi-tenant
* Tenant-specific ad-hoc integration logic.
* Full audit logs.
* Auto detection of 3rd party business apps.
* API.

[Learn more...] (http://www.lokad.com/salescast-sales-forecasting-software.ashx)

## Callcalc
Callcalc is an email-based forecasting client for call centers. You send an Excel spreadsheet with call volumes and Callcalc replies with forecasts.

Lokad.CQRS provides simple and reliable foundation for a heavily verticalized solution that tailors our raw Forecasting API for the very specific needs of call centers.

![] (chapter-02-content/figure-02-01.png)

**Features:**

* Multi-tenant
* Multiple calling queues
* Erlang-C staffing optimization
* IMAP interface

[Learn more...] (http://www.lokad.com/call-center-calculator-software.ashx)

## Lokad Hub
Lokad.Hub is an internal platform unifying metered pay-as-you-go forecasting subscription offered by Lokad. 

Lokad.CQRS is the key to transition a pre-cloud business app toward a decoupled and efficient design. It provides additional reliability and allows much faster development iterations. 

![] (chapter-02-content/figure-02-02.png)

**Features:**

* Multi-tenant.
* Full audit logs.
* Flexible reporting, including ad-hoc reports and temporal queries.
* Reliable failure handling.
* No-downtime upgrades.
* Integration with payment, Geo-Location and CRM systems.

## Public References
There already are a few brave souls that have tried to apply Lokad.CQRS for Windows Azure outside of our company. Here is some feedback that was kindly shared with us.

**David Pfeffer** (<http://twitter.com/#!/bytenik>)

_fivepm technolgy, inc. uses Lokad.Cqrs to drive its entire flagship Treadmarks product. We rely on the service bus to reliably deliver messages across our cloud application, and we use the view persistence helpers to store any view data. We initially investigated rolling our own framework or using NServiceBus, but the maturity of Lokad.Cqrs on the Azure platform made it an easy winner over the other possibilities._

**Chris Martin** (<http://twitter.com/#!/cmartinbot>):
_I can't thank Lokad enough for the work you guys have put in. You have literally saved our startup thousands of man-hours!_

> If you want to tell us a story, use case or share a success, please, don't hesitate to drop us a line to <contact@lokad.com>.




