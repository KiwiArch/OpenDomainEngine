# OpenDomainEngine aka Ode

.Net framework to support domain driven design (DDD) using CQS/CQRS style implementations.

## Intro..

A few years ago I was involved in several large scale projects following the philosophy of domain driven design (DDD) as presented by Eric Evans.

The implementation of these projects soon started to pick up technical approaches along the lines of CQRS; and whilst this felt right as the target solution architecture we found ourselves spending far too much time and energy on infrastructure and plumbing rather than being focused on the domain, rather defeating the original point of encompassing DDD.

The domain engine came after these projects, as an experiment and sandbox for my own some of my colleagues ideas on how to avoid this in future projects as well as solving some of the technical problems we'd run into on those projects. This wasn't intended as production code but versions prior to this have found their way into the wild and are running 24/7 services.  

So what does the domain engine do?, essentialy it allows for domains to be coded and tested without any dependencoes on infrastructure code.   The guide coves more of the design goals but ideally I wanted to be able to write aggregates, event handlers and process managers to encapsulate a domain without a single dependency on infrastructure code, assemblies or packages.


