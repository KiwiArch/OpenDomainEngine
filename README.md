# OpenDomainEngine aka Ode

.Net framework to support clean domain driven design (DDD) using CQS/CQRS style implementations without infrastructure plumbing in the domain.

## Intro..

A few years ago I was involved in several large scale projects following the philosophy of domain driven design (DDD) as presented by Eric Evans.

The implementation of these projects soon started to pick up technical approaches along the lines of CQRS; and whilst this felt right as the target solution architecture we found ourselves spending far too much time and energy on infrastructure and plumbing rather than being focused on the domain, rather defeating the original point of encompassing DDD.

The domain engine came after these projects, as an experiment and sandbox for my own and some of my colleagues ideas on how to avoid this in the future as well as solving some of the technical problems we'd run into on those earlier endeavours. This wasn't originly intended as production code however versions have found their way into the wild and are running 24/7 services.  

So what does the domain engine do?, essentialy it allows for domains to be coded and tested without any dependencoes on infrastructure code.   The wiki covers more of the design goals but ideally I wanted to be able to write aggregates, event handlers and process managers to encapsulate a domain without a single dependency on infrastructure code, assemblies or packages.  Idealy I wanted aggregates that look like this:

```
namespace Acme.Warehouse.Locations
{
    public class Location
    {
        public LocationCreated When(CreateLocation command) {...}
    
        public AdjustedIn When(AdjustIn command) {...}

        public MovedIn When(MoveIn command) {...}

        public MovedOut When(MoveOut command) {...}

        protected LocationCreated Then(LocationCreated stateChange) {...}

        protected AdjustedIn Then(AdjustedIn stateChange) {...}

        protected MovedIn Then(MovedIn stateChange) {...}

        protected MovedOut Then(MovedOut stateChange) {...}
    }
}
```

with no dependancies on base classes or interfaces or any other plumbing, just the domain implementation pure and simple.

Secondly I wanted to be able to test domain models; to go beyond unit tests of singular methods to test business processes involving interactions between process managers, aggregates, event handlers etc. but without including layers of infrastructure, i.e. I don’t need to test Service Fabric just that my domain model behaves as expected.  For this the domain engine is written to be completely infrastructure agnostic, it can run an entire bounded context in a console app, as distributed micro services on Service Fabric, as Restful webservices backed by GYES or whatever the problem space requires.  



