using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ode.Domain.Engine.SampleModel.Locations;

namespace Ode.Domain.Engine.JsonSerialization.Tests
{
    [TestClass]
    public class EventSerializationTests
    {
        [TestMethod]
        public void CreateEventAdapterTest()
        {
            string aggregateId = "Box";
            int aggregateVersion = 2;
            string aggregateType = typeof(Location).FullName;
            string commandId = Guid.NewGuid().ToString();
            string correlationId = commandId;

            LocationCreated locationCreatedEvent = new LocationCreated(aggregateId, string.Empty);

            string jsonEvent = new EventSerialization().SerializeEvent(locationCreatedEvent);

            var eventAdapter = new EventSerialization().DeserializeEvent(aggregateId, aggregateVersion, aggregateType, commandId, correlationId, jsonEvent);

            Assert.IsInstanceOfType(eventAdapter, typeof(IEvent));
            Assert.IsInstanceOfType(eventAdapter, typeof(IEvent<LocationCreated>));
        }

        [TestMethod]
        public void DeserializedEventAdapterAsFulyTypedTest()
        {
            string aggregateId = "Box";
            int aggregateVersion = 2;
            string aggregateType = typeof(Location).FullName;
            string commandId = Guid.NewGuid().ToString();
            string correlationId = commandId;

            var locationCreated = new LocationCreated(aggregateId, string.Empty);

            string serializedLocationCreated = new EventSerialization().SerializeEvent(locationCreated);

            var eventAdapter = new EventSerialization().DeserializeEvent(aggregateId, aggregateVersion, aggregateType, commandId, correlationId, serializedLocationCreated);

            var fullyTypedEventAdapter = eventAdapter.AsFullyTypedEvent(eventAdapter.EventBody);

            Assert.IsInstanceOfType(fullyTypedEventAdapter, typeof(IEvent<LocationCreated>));
        }
    }
}