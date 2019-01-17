using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ode.Domain.Engine.Factories;
using Ode.Domain.Engine.Model.Fakes;
using Ode.Domain.Engine.MsSqlServer.Exceptions;
using Ode.Domain.Engine.Repositories;
using Ode.Domain.Engine.SampleModel;
using Ode.Domain.Engine.SampleModel.InventoryItems;
using Ode.Domain.Engine.SampleModel.Locations;
using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ode.Domain.Engine.MsSqlServer.Repositories.Tests
{
    [TestClass()]
    public class SqlEventStoreTests
    {
        private StubIResolveEventTypes contextStub;
        private const string connectionString = @"Server=localhost;Database=SqlEventStoreTests;Trusted_Connection=true";

        private class TestModel
        {
            public readonly string[] AggregateIds = new string[3] { "ar1", "ar2", "ar3" };
            public readonly string[] CommandIds = new string[6] { "c1", "c2", "c3", "c4", "c5", "c6" };
            public readonly StockItem[] Items = new StockItem[2] { new StockItem("item", "123"), new StockItem("item2", "456") };

            public TestModel()
            {
                AllEvents = CreateEventHistory();
            }

            private List<IEvent> CreateEventHistory()
            {
                var allEvents = new List<IEvent>();

                string firstAdjustmemt = $"adjustment_{Guid.NewGuid()}";
                string secondAdjustmemt = $"adjustment_{Guid.NewGuid()}";

                string firstMovement = $"movement_{Guid.NewGuid()}";

                // First location is created 
                allEvents.Add(EventFactory.Default.CreateEvent<Location, LocationCreated>(AggregateIds[0], 1, CommandIds[0], CommandIds[0], new LocationCreated(AggregateIds[0], string.Empty)));
                // First location seeded
                allEvents.Add(EventFactory.Default.CreateEvent<Location, AdjustedIn>(AggregateIds[0], 2, CommandIds[1], CommandIds[1], new AdjustedIn(firstAdjustmemt, AggregateIds[0], Items[0])));

                // Second location is created and item moved in from first location
                allEvents.Add(EventFactory.Default.CreateEvent<Location, LocationCreated>(AggregateIds[1], 1, CommandIds[2], CommandIds[2], new LocationCreated(AggregateIds[1], string.Empty)));
                allEvents.Add(EventFactory.Default.CreateEvent<Location, MovedOut>(AggregateIds[0], 3, CommandIds[3], CommandIds[3], new MovedOut(firstMovement, AggregateIds[0], Items[0], AggregateIds[1])));
                allEvents.Add(EventFactory.Default.CreateEvent<Location, MovedIn>(AggregateIds[1], 2, CommandIds[4], CommandIds[3], new MovedIn(firstMovement, AggregateIds[1], Items[0], AggregateIds[0])));

                // Third location created and seeded then moved to first location 
                allEvents.Add(EventFactory.Default.CreateEvent<Location, LocationCreated>(AggregateIds[2], 1, CommandIds[4], CommandIds[4], new LocationCreated(AggregateIds[2], string.Empty)));
                allEvents.Add(EventFactory.Default.CreateEvent<Location, AdjustedIn>(AggregateIds[2], 2, CommandIds[5], CommandIds[5], new AdjustedIn(secondAdjustmemt, AggregateIds[2], Items[1])));

                return allEvents;
            }

            public IEnumerable<IEvent> AllEvents { get; private set; }


        }

        [TestInitialize]
        public void TestInitialize()
        {
            using (var context = new EventContext(connectionString))
            {
                context.Database.EnsureDeleted();

                context.Database.EnsureCreated();
            }

            contextStub = new StubIResolveEventTypes();
            contextStub.ResolveEventTypeString = (s) => Type.GetType(s);
            contextStub.ResolveEventTypeFullNameType = (t) => t.AssemblyQualifiedName;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using (var context = new EventContext(connectionString))
            {
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod()]
        public void SqlEventStoreTest()
        {
            Assert.IsInstanceOfType(new SqlEventStore(connectionString), typeof(IEventStore));
        }

        [TestMethod()]
        public void ConfigureTest()
        {
            Assert.IsNotNull(new SqlEventStore(connectionString));
        }

        [TestMethod()]
        public void RetrieveByIdNoMatchesTest()
        {
            var sqlEventStore = new SqlEventStore(connectionString);

            var events = sqlEventStore.RetrieveById(Guid.NewGuid().ToString());

            Assert.IsNotNull(events);
        }

        [TestMethod()]
        public void StoreTest()
        {
            var sqlEventStore = new SqlEventStore(connectionString);

            var testModel = new TestModel();

            foreach (var id in testModel.AllEvents.Select(e => e.AggregateId).Distinct())
            {
                sqlEventStore.Store(id, testModel.AllEvents.Where(e => e.AggregateId == id));
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void StoreToExistingStreamTest()
        {
            var sqlEventStore = new SqlEventStore(connectionString);

            var testModel = new TestModel();

            var aggregateId = testModel.AggregateIds[0];
            var aggregateEvents = testModel.AllEvents.Where(e => e.AggregateId == aggregateId);

            sqlEventStore.Store(aggregateId, aggregateEvents.Take(1));

            sqlEventStore.Store(aggregateId, aggregateEvents.Skip(1).Take(1));

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void RetrieveByIdReturnsEventsTest()
        {
            var testModel = new TestModel();

            var sqlEventStore = new SqlEventStore(connectionString);

            foreach (var id in testModel.AllEvents.Select(e => e.AggregateId).Distinct())
            {
                sqlEventStore.Store(id, testModel.AllEvents.Where(e => e.AggregateId == id));
            }

            var events = sqlEventStore.RetrieveById(testModel.AggregateIds[0]);

            Assert.IsTrue(events.Count() == testModel.AllEvents.Count(e => e.AggregateId == testModel.AggregateIds[0]) && events.All(e => e.AggregateId == testModel.AggregateIds[0]));
        }

        [TestMethod]
        public void RetrieveByIdReturnsCorrectlyTypedEventsTest()
        {
            var testModel = new TestModel();

            var sqlEventStore = new SqlEventStore(connectionString);

            foreach (var id in testModel.AllEvents.Select(e => e.AggregateId).Distinct())
            {
                sqlEventStore.Store(id, testModel.AllEvents.Where(e => e.AggregateId == id));
            }

            var events = sqlEventStore.RetrieveById(testModel.AggregateIds[0]);

            Assert.IsInstanceOfType(events.First(), typeof(IEvent<LocationCreated>));
        }

        [TestMethod]
        public void RetrieveByIdReturnsEventsInOrderTest()
        {
            var testModel = new TestModel();

            var sqlEventStore = new SqlEventStore(connectionString);

            foreach (var id in testModel.AllEvents.Select(e => e.AggregateId).Distinct())
            {
                sqlEventStore.Store(id, testModel.AllEvents.Where(e => e.AggregateId == id));
            }

            var events = sqlEventStore.RetrieveById(testModel.AggregateIds[0]);

            Assert.IsTrue(events.Select(e => e.AggregateVersion).SequenceEqual(Enumerable.Range(1, events.Count())));
        }

        [TestMethod]
        public void RetrieveByIdReturnsEventsUsingDifferantInstancesTest()
        {
            var testModel = new TestModel();

            var sqlEventStoreA = new SqlEventStore(connectionString);

            foreach (var id in testModel.AllEvents.Select(e => e.AggregateId).Distinct())
            {
                sqlEventStoreA.Store(id, testModel.AllEvents.Where(e => e.AggregateId == id));
            }

            var sqlEventStoreB = new SqlEventStore(connectionString);

            var events = sqlEventStoreB.RetrieveById(testModel.AggregateIds[1]);

            Assert.IsTrue(events.Count() == testModel.AllEvents.Count(e => e.AggregateId == testModel.AggregateIds[1]) && events.All(e => e.AggregateId == testModel.AggregateIds[1]));
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StoreFailsIfSteamIdNotSuppliedTest()
        {
            var testModel = new TestModel();

            new SqlEventStore(connectionString).Store(string.Empty, testModel.AllEvents);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StoreFailsIfEventsNull()
        {
            new SqlEventStore(connectionString).Store(Guid.NewGuid().ToString(), null);
        }

        [TestMethod, ExpectedException(typeof(EventStreamConcurrencyException))]
        public void StoreFailsIfAggregeteVersionExistsTest()
        {
            var testModel = new TestModel();

            var sqlEventStoreA = new SqlEventStore(connectionString);

            var events = testModel.AllEvents.Where(e => e.AggregateId == testModel.AggregateIds.First());
            var aggregateId = events.First().AggregateId;

            sqlEventStoreA.Store(aggregateId, events);

            var item = new StockItem("item", "123");

            var commandId = Guid.NewGuid().ToString();
            var nextEvent = EventFactory.Default.CreateEvent<Location, AdjustedIn>(aggregateId, events.Last().AggregateVersion, commandId, commandId, new AdjustedIn($"adjustment_{Guid.NewGuid()}", aggregateId, item));

            new SqlEventStore(connectionString).Store(aggregateId, new IEvent[] { nextEvent });
        }

        [TestMethod, ExpectedException(typeof(EventStreamConcurrencyException))]
        public void StoreFailsIfAggregeteVersionNotConcurrentTest()
        {
            var testModel = new TestModel();

            var sqlEventStoreA = new SqlEventStore(connectionString);

            var events = testModel.AllEvents.Where(e => e.AggregateId == testModel.AggregateIds.First());
            var aggregateId = events.First().AggregateId;

            sqlEventStoreA.Store(aggregateId, events);

            var item = new StockItem("item", "123");

            var commandId = Guid.NewGuid().ToString();
            var nextEvent = EventFactory.Default.CreateEvent<Location, AdjustedIn>(aggregateId, events.Last().AggregateVersion + 2, commandId, commandId, new AdjustedIn($"adjustment_{Guid.NewGuid()}", aggregateId, item));

            new SqlEventStore(connectionString).Store(aggregateId, new IEvent[] { nextEvent });
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void StoreFailsIfAggregeteTypesNotConsistentTest()
        {
            var testModel = new TestModel();

            var sqlEventStoreA = new SqlEventStore(connectionString);

            var events = testModel.AllEvents.Where(e => e.AggregateId == testModel.AggregateIds.First());
            var aggregateId = events.First().AggregateId;

            sqlEventStoreA.Store(aggregateId, events);

            var commandId = Guid.NewGuid().ToString();

            var nextEvents = new List<IEvent>();

            var item = new StockItem("item", "123");

            nextEvents.Add(EventFactory.Default.CreateEvent<Location, AdjustedIn>(aggregateId, events.Last().AggregateVersion + 1, commandId, commandId, new AdjustedIn($"adjustment_{Guid.NewGuid()}", aggregateId, item)));
            nextEvents.Add(EventFactory.Default.CreateEvent<InventoryItem, InventoryItemCreated>(aggregateId, events.Last().AggregateVersion + 2, commandId, commandId, new InventoryItemCreated(aggregateId)));
            nextEvents.Add(EventFactory.Default.CreateEvent<Location, AdjustedIn>(aggregateId, events.Last().AggregateVersion + 3, commandId, commandId, new AdjustedIn($"adjustment_{Guid.NewGuid()}", aggregateId, item)));

            new SqlEventStore(connectionString).Store(aggregateId, nextEvents);
        }
    }
}