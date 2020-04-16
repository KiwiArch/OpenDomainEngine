namespace Ode.Domain.Engine.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ode.Domain.Engine.SampleModel.Locations;
    using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;
    using Ode.Domain.Engine.SampleModel.Movements;
    using Ode.Domain.Engine;
    using Ode.Domain.Engine.Dispatchers;
    using Ode.Domain.Engine.Factories;
    using Ode.Domain.Engine.Model;
    using Ode.Domain.Engine.Model.Configuration;
    using Ode.Domain.Engine.Repositories;
    using Moq;
    using System.Net.WebSockets;

    [TestClass]
    public class DistributedEngineTests
    {
        private IBoundedContextModel boundedContextModel;
        private Mock<IEventStore> eventStore;

        [TestInitialize]
        public void TestInitialize()
        {
            this.boundedContextModel = new BoundedContextModel().WithAssemblyContaining<Location>().WithEventHandler<MovedOut, Movement, MoveIn>(e => e.EventBody.Movement, c => c.Location);
            this.eventStore = new Mock<IEventStore>(MockBehavior.Loose);
        }

        [TestMethod]
        public void CreateCommandEngine()
        {
            var commandEngine = DomainFactory.CreateCommandEngine(this.boundedContextModel, this.eventStore.Object);
        }

        [TestMethod]
        public void ProcessCommandToEvent()
        {
            var location = "Mayfair";

            var command = CommandFactory.Default.CreateCommand("command1", location, new CreateLocation(location));

            var commandEngine = DomainFactory.CreateCommandEngine(this.boundedContextModel, eventStore.Object);

            var events = commandEngine.Process(command);

            Assert.IsNotNull(@events.SingleOrDefault(e => e.EventBody is LocationCreated));

            this.eventStore.Verify(x => x.Store(location, events));
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

            var eventHandler = new Mock<IEventHandler>();

            var distributionEngine = DomainFactory.CreateEventDispatcher(this.boundedContextModel, eventHandler.Object);

            distributionEngine.DispatchEvent(@event);

            eventHandler.Verify(x => x.Handle(@event, eventHandlerId, typeof(Movement)));
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

            var eventEngine = DomainFactory.CreateEventHandler(this.boundedContextModel, this.eventStore.Object);

            var commands = eventEngine.Handle(@event, eventHandlerId, typeof(Movement));

            Assert.IsTrue(commands.Count() == 0);

            this.eventStore.Verify(x => x.Store(storedEventStreamId, It.Is<IEnumerable<IEvent>>(x => x.First().EventBody as MovedIn == @event.EventBody)));
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

            var eventEngine = DomainFactory.CreateEventHandler(this.boundedContextModel, this.eventStore.Object);

            var commands = eventEngine.Handle(@event, eventHandlerId, typeof(Movement));

            Assert.IsTrue(commands.Count() == 1);

            this.eventStore.Verify(x => x.Store(storedEventStreamId, It.Is<IEnumerable<IEvent>>(x => x.First().EventBody as MovedOut == @event.EventBody)));

            Assert.IsTrue(commands.Single().AggregateId == toLocation);
            Assert.IsTrue(commands.Single().CorrelationId == @event.CorrelationId);
        }
    }
}
