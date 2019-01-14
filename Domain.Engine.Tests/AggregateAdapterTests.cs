using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ode.Domain.Engine.SampleModel.Locations;
using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;
using Ode.Domain.Engine.Factories;
using Ode.Domain.Engine.Model;
using Ode.Domain.Engine.Model.Configuration;

namespace Ode.Domain.Engine.Tests
{
    [TestClass]
    public class AggregateAdapterTests
    {
        [TestMethod]
        public void CommandReturnsAssociatedEvent()
        {
            var aggregateRoot = new Location();
            var command = new CreateLocation("locationName");

            var contextMap = new BoundedContextModel().WithAssemblyContaining<Location>();

            var aggregateAdapter = AggregateAdapterFactory.Default.CreateAggregate(contextMap, aggregateRoot);
            var commandAdapter = CommandFactory.Default.CreateCommand<CreateLocation>("command1", "correlationId", "aggregateId", command);

            var events = aggregateAdapter.ProcessCommand(commandAdapter);

            Assert.IsNotNull(events);

            var locationCreatedDomainEvent = events.ToList().FirstOrDefault();

            Assert.IsInstanceOfType(locationCreatedDomainEvent, typeof(IEvent));
            Assert.IsInstanceOfType((locationCreatedDomainEvent as IEvent).EventBody, typeof(LocationCreated));
        }

        [TestMethod]
        public void RehydrateAggregate()
        {
            var aggregateId = Guid.NewGuid().ToString();
            var locationName = "location1";
            var item = new StockItem("item1", "1");

            var contextMap = new BoundedContextModel().WithAssemblyContaining<Location>();

            var aggregateRoot = new Location();
            var aggregateAdapter = AggregateAdapterFactory.Default.CreateAggregate(contextMap, aggregateRoot).WithId(aggregateId);

            var eventAdapterFactory = new EventFactory();

            var aggregateEventHistory = new List<IEvent>();
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, LocationCreated>(aggregateId, 1, string.Empty, string.Empty, new LocationCreated(locationName, string.Empty)));
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, AdjustedIn>(aggregateId, 2, string.Empty, string.Empty, new AdjustedIn($"adjustment_{Guid.NewGuid()}", locationName, item)));
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, MovedOut>(aggregateId, 3, string.Empty, string.Empty, new MovedOut($"movement_{Guid.NewGuid()}", locationName, item, "toLocationName")));


            aggregateAdapter.Rehydrate(aggregateEventHistory);
        }

        [TestMethod]
        public void RehydrateAggregateFailsIfEventVersionsNonSequential()
        {
            var aggregateId = Guid.NewGuid().ToString();
            var locationName = "location1";
            var item = new StockItem("item1", "1");

            var contextMap = new BoundedContextModel().WithAssemblyContaining<Location>();

            var aggregateRoot = new Location();
            var aggregateAdapter = AggregateAdapterFactory.Default.CreateAggregate(contextMap, aggregateRoot).WithId(aggregateId);

            var eventAdapterFactory = new EventFactory();

            var aggregateEventHistory = new List<IEvent>();
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, LocationCreated>(aggregateId, 1, string.Empty, string.Empty, new LocationCreated(locationName, string.Empty)));
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, AdjustedIn>(aggregateId, 1, string.Empty, string.Empty, new AdjustedIn($"adjustment_{Guid.NewGuid()}", locationName, item)));
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, MovedOut>(aggregateId, 2, string.Empty, string.Empty, new MovedOut($"movement_{Guid.NewGuid()}", locationName, item, "toLocationName")));

            Assert.ThrowsException<ArgumentException>(() => aggregateAdapter.Rehydrate(aggregateEventHistory));
        }

        [TestMethod]
        public void RehydrateAggregateFailsIfEventAggregateIdMismatch()
        {
            var aggregateId = Guid.NewGuid().ToString();
            var locationName = "location1";
            var item = new StockItem("item1", "1");

            var contextMap = new BoundedContextModel().WithAssemblyContaining<Location>();

            var aggregateRoot = new Location();
            var aggregateAdapter = AggregateAdapterFactory.Default.CreateAggregate(contextMap, aggregateRoot).WithId(aggregateId);

            var eventAdapterFactory = new EventFactory();

            var aggregateEventHistory = new List<IEvent>();
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, LocationCreated>(aggregateId, 1, string.Empty, string.Empty, new LocationCreated(locationName, string.Empty)));
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, AdjustedIn>(aggregateId, 2, string.Empty, string.Empty, new AdjustedIn($"adjustment_{Guid.NewGuid()}", locationName, item)));
            aggregateEventHistory.Add(eventAdapterFactory.CreateEvent<Location, MovedOut>(Guid.NewGuid().ToString(), 3, string.Empty, string.Empty, new MovedOut($"movement_{Guid.NewGuid()}", locationName, item, "toLocationName")));

            Assert.ThrowsException<ArgumentException>(() => aggregateAdapter.Rehydrate(aggregateEventHistory));
        }
    }
}
