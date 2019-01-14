namespace Ode.Domain.EngineTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.QualityTools.Testing.Fakes.Stubs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ode.Domain.Engine.SampleModel.Locations;
    using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;
    using Ode.Domain.Engine.SampleModel.Movements;
    using Ode.Domain.Engine;
    using Ode.Domain.Engine.Dispatchers;
    using Ode.Domain.Engine.Factories;
    using Ode.Domain.Engine.Fakes;
    using Ode.Domain.Engine.Model;
    using Ode.Domain.Engine.Model.Configuration;
    using Ode.Domain.Engine.Repositories;
    using Ode.Domain.Engine.Repositories.Fakes;

    [TestClass]
    public class DistributedEngineTests
    {
        private IBoundedContextModel boundedContextModel;
        private StubIEventStore eventStore;
        private StubObserver eventStoreObserver;

        [TestInitialize]
        public void TestInitialize()
        {
            this.boundedContextModel = new BoundedContextModel().WithAssemblyContaining<Location>().WithEventHandler<MovedOut, Movement, MoveIn>(e => e.EventBody.Movement, c => c.Location);
            this.eventStore = new StubIEventStore();
            this.eventStoreObserver = new StubObserver();
            this.eventStore.InstanceObserver = eventStoreObserver;
        }

        [TestMethod]
        public void CreateCommandEngine()
        {
            var commandEngine = DomainFactory.CreateCommandEngine(this.boundedContextModel, this.eventStore);
        }

        [TestMethod]
        public void ProcessCommandToEvent()
        {
            var location = "Mayfair";

            var command = CommandFactory.Default.CreateCommand("command1", location, new CreateLocation(location));

            var commandEngine = DomainFactory.CreateCommandEngine(this.boundedContextModel, eventStore);

            var events = commandEngine.Process(command);

            Assert.IsNotNull(@events.SingleOrDefault(e => e.EventBody is LocationCreated));
            Assert.IsTrue(this.eventStoreObserver.GetCalls().Any(call => call.StubbedMethod.Name == nameof(IEventStore.Store)));
            Assert.IsTrue(this.eventStoreObserver.GetCalls().Any(call => call.StubbedMethod.Name == nameof(IEventStore.Store) && call.GetArguments().First().ToString() == location));
            Assert.IsTrue(this.eventStoreObserver.GetCalls().Any(call => call.StubbedMethod.Name == nameof(IEventStore.Store) && call.GetArguments().Where(a => a is IEnumerable<IEvent>).Select(a => a as IEnumerable<IEvent>).First().Intersect(events).Count() == events.Count()));
        }

        [TestMethod]
        public void DistributeEventsToHandlers()
        {
            var movement = "movement";
            var location = "Mayfair";
            var item = new StockItem("item", "1");
            var toLocation = "toLocation";
            var eventHandlerId = $"{typeof(Movement).Name}\\{movement}";

            var @event = EventFactory.Default.CreateEvent<Location, MovedOut>(location, 1, "command1", "command1", new MovedOut(movement, location, item, toLocation));

            var eventHandlerStub = new StubIEventHandler();
            var eventHandlerObserver = new StubObserver();
            eventHandlerStub.InstanceObserver = eventHandlerObserver;

            var distributionEngine = DomainFactory.CreateEventDispatcher(this.boundedContextModel, eventHandlerStub);

            distributionEngine.DispatchEvent(@event);

            var eventHandlerCalls = eventHandlerObserver.GetCalls().Where(call => call.StubbedMethod.Name == nameof(IEventHandler.Handle));

            Assert.IsTrue(eventHandlerCalls.Any());

            Assert.IsTrue(eventHandlerCalls.Any(call => call.GetArguments().Any(a => a == @event) && call.GetArguments().Any(a => a as Type == typeof(Movement)) && call.GetArguments().Any(a => a.ToString() == eventHandlerId)));

        }

        [TestMethod]
        public void ProcessEvent()
        {
            var movement = "movementId";
            var location = "location";
            var item = new StockItem("item", "1");

            var fromLocation = "fromLocationId";
            var eventHandlerId = $"{typeof(Movement).Name}\\{movement}";
            var storedEventStreamId = eventHandlerId;

            var @event = EventFactory.Default.CreateEvent<Location, MovedIn>(location, 3, "commandId", "correlationId", new MovedIn(movement, location, item, fromLocation));

            var eventEngine = DomainFactory.CreateEventHandler(this.boundedContextModel, this.eventStore);

            var commands = eventEngine.Handle(@event, eventHandlerId, typeof(Movement));

            Assert.IsTrue(commands.Count() == 0);

            var callToStore = this.eventStoreObserver.GetCalls().Where(call => call.StubbedMethod.Name == nameof(IEventStore.Store)).SingleOrDefault();

            Assert.IsNotNull(callToStore);

            Assert.IsTrue(callToStore.GetArguments().Any(a => a.ToString() == storedEventStreamId) && callToStore.GetArguments().Where(a => a is IEnumerable<IEvent>).Select(a => a as IEnumerable<IEvent>).Any(a => a.Count() == 1 && a.First().EventBody == @event.EventBody));
        }

        [TestMethod]
        public void ProcessEventToCommand()
        {
            var movement = "movementId";
            var location = "location";
            var item = new StockItem("item", "1");
            var toLocation = "toLocation";
            var eventHandlerId = $"{typeof(Movement).Name}\\{movement}";
            var storedEventStreamId = eventHandlerId;

            var @event = EventFactory.Default.CreateEvent<Location, MovedOut>(location, 3, "commandId", "correlationId", new MovedOut(movement, location, item, toLocation));

            var eventEngine = DomainFactory.CreateEventHandler(this.boundedContextModel, this.eventStore);

            var commands = eventEngine.Handle(@event, eventHandlerId, typeof(Movement));

            Assert.IsTrue(commands.Count() == 1);

            var callToStore = this.eventStoreObserver.GetCalls().Where(call => call.StubbedMethod.Name == nameof(IEventStore.Store)).SingleOrDefault();

            Assert.IsNotNull(callToStore);

            Assert.IsTrue(callToStore.GetArguments().Any(a => a.ToString() == storedEventStreamId) && callToStore.GetArguments().Where(a => a is IEnumerable<IEvent>).Select(a => a as IEnumerable<IEvent>).Any(a => a.Count() == 1 && a.First().EventBody == @event.EventBody));

            Assert.IsTrue(commands.Single().AggregateId == toLocation);
            Assert.IsTrue(commands.Single().CorrelationId == @event.CorrelationId);
        }

        [TestMethod]
        public void ProcessCommandToProjection()
        {
            var boundedContext = new BoundedContextModel().WithAssemblyContaining<Location>();
            var eventStore = new StubIEventStore();

            var location = "Mayfair";

            var command = CommandFactory.Default.CreateCommand("command1", location, new CreateLocation(location));

            var eventHandler = new TransactionalEventHandler(boundedContext, eventStore, cacheRuntimeModel: false);

            var eventDispatcher = new EventDispatcher(boundedContext, eventHandler);

            var commandEngine = DomainFactory.CreateCommandEngine(boundedContext, eventStore, eventHandler);

            var events = commandEngine.Process(command);

            Assert.Inconclusive();
        }
    }
}
