namespace Ode.Domain.EngineTests
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ode.Domain.Engine.SampleModel.Locations;
    using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;
    using Ode.Domain.Engine.SampleModel.Movements;
    using Ode.Domain.Engine.Adapters;
    using Ode.Domain.Engine.Factories;
    using Ode.Domain.Engine.Model;

    [TestClass]
    public class EventHandlerAdapterTests
    {
        [TestMethod]
        public void ProcessEventWithRehydrating()
        {
            var boundedContextModel = new BoundedContextModel().WithEventHandler<MovedOut, Movement, MoveIn>(e => e.EventBody.Movement, c => c.Location);

            var eventHandlerAdapter = new EventHandlerAdapter<Movement>(new Movement(), boundedContextModel).WithId("movement");

            var item = new StockItem("item", "123");

            var myHistoryEvent = EventFactory.Default.CreateEvent<Location, MovedOut>("sourceLocation", 2, "CommandId", "CoorrelationId", new MovedOut("movement", "sourceLocation", item, "destinationLocation"));
            eventHandlerAdapter.Rehydrate(new[] { myHistoryEvent });

            var myEvent = EventFactory.Default.CreateEvent<Location, MovedIn>("destinationLocation", 3, "CommandId2", "CoorrelationId2", new MovedIn("movement", "destinationLocation", item, "sourceLocation"));
            eventHandlerAdapter.ProcessEvent(myEvent);

            Assert.AreEqual(1, eventHandlerAdapter.UncommittedChanges.Count());
            Assert.AreEqual(2, eventHandlerAdapter.UncommittedChanges.First().AggregateVersion);
        }

        [TestMethod]
        public void DeserializedEventAdapterAsFulyTypedTest()
        {
            string aggregateId = "Box";
            int aggregateVersion = 2;
            string aggregateType = typeof(Location).FullName;
            string commandId = Guid.NewGuid().ToString();
            string correlationId = commandId;

            CreateLocation createLocation = new CreateLocation(aggregateId);

            var eventAdapter = EventFactory.Default.CreateEvent(aggregateId, aggregateVersion, aggregateType, commandId, correlationId, createLocation);

            var fullyTypedEventAdapter = eventAdapter.AsFullyTypedEvent(eventAdapter.EventBody);

            Assert.IsTrue(fullyTypedEventAdapter.EventBody.Location == aggregateId);
        }
    }
}
