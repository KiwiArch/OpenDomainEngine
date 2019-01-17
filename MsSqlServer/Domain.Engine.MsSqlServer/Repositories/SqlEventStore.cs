namespace Ode.Domain.Engine.MsSqlServer.Repositories
{
    using Engine.Repositories;
    using Exceptions;
    using Model;
    using Serialization;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Transactions;

    public class SqlEventStore : IEventStore, ISnapshotRepository
    {
        private readonly ISerializeEvents eventSerialization;
        private readonly ISerializeSnapshots snapshotSerialization;

        private string connectionString;
        private string dbSchema;
        private string eventTableName;
        private string snapshotTableName;

        public SqlEventStore(string connectionString)
            : this(connectionString, new JsonSerialization.EventSerialization(), new JsonSerialization.SnapshotSerialization())
        {
        }

        public SqlEventStore(string connectionString, ISerializeEvents eventSerialization, ISerializeSnapshots snapshotSerialization)
        {
            this.connectionString = connectionString;
            this.eventSerialization = eventSerialization;
            this.snapshotSerialization = snapshotSerialization;
        }

        public SqlEventStore WithSchema(string dbSchema)
        {
            this.dbSchema = dbSchema;
            return this;
        }

        public SqlEventStore WithEventTable(string eventTableName)
        {
            this.eventTableName = eventTableName;
            return this;
        }

        public SqlEventStore WithSnapshotTable(string snapshotTableName)
        {
            this.snapshotTableName = snapshotTableName;
            return this;
        }

        public SqlEventStore WithCreateDatabase()
        {
            using (var databaseContext = CreateEventContext())
            {
                databaseContext.Database.EnsureCreated();
            }

            return this;
        }

        public IEnumerable<IEvent> RetrieveById(string streamId)
        {
            return RetrieveById(streamId, 0);
        }

        public IEnumerable<IEvent> RetrieveById(string streamId, int fromVersion)
        {
            var result = new Collection<IEvent>();

            using (var eventStore = CreateEventContext())
            {
                foreach (var item in eventStore.Events.Where(e => e.StreamId == streamId && e.Version > fromVersion).OrderBy(e => e.Version))
                {
                    var eventAdapter = eventSerialization.DeserializeEvent(item.StreamId, item.Version, item.StreamType, item.CommandId, item.CorrelationId, item.EventData);

                    result.Add(eventAdapter);
                }
            }

            return result;
        }

        public void Store(string streamId, IEnumerable<IEvent> events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            if (string.IsNullOrWhiteSpace(streamId))
            {
                throw new ArgumentNullException(nameof(streamId));
            }

            if (events.Count() == 0)
            {
                return;
            }

            events = events.OrderBy(e => e.AggregateVersion);


            using (var transactionScope = DomainTransaction.DefaultTransactionScope())
            {

                using (var eventStore = CreateEventContext())
                {
                    var q = (from e in eventStore.Events
                             where e.StreamId == streamId
                             orderby e.Version descending
                             select new { ExpectedNextVersion = e.Version + 1, ExpectedAggregateType = e.StreamType });


                    var expectedValues = q.FirstOrDefault() ?? new { ExpectedNextVersion = 1, ExpectedAggregateType = events.First().AggregateType };

                    SqlEventStore.GuardStreamConcurency(events.First(), expectedValues.ExpectedNextVersion);

                    SqlEventStore.GuardEventArgregateTypes(events, expectedValues.ExpectedAggregateType);

                    SqlEventStore.GuardEventVersions(events, expectedValues.ExpectedNextVersion);

                    foreach (var item in events)
                    {
                        eventStore.Events.Add(ConvertEvent(streamId, item));
                    }

                    eventStore.SaveChanges();
                }

                transactionScope.Complete();
            }
        }

        public ISnapshot<TAggregate> Retrieve<TAggregate>(string id)
        {
            using (var context = CreateEventContext())
            {
                var snapshot = context.Snapshots.Where(s => s.SnapshotId == id).OrderByDescending(e => e.Version).FirstOrDefault();

                if (snapshot != null)
                {
                    return new Snapshot<TAggregate>() { SnapshotId = snapshot.SnapshotId, Version = snapshot.Version, Aggregate = snapshotSerialization.DeserializeSnapshot<TAggregate>(snapshot.SnapshotData) };
                }

                return null;
            }
        }

        public void Store<TAggregate>(string aggregateId, int aggregateVersion, TAggregate aggregate)
        {
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate));
            }

            using (var transactionScope = DomainTransaction.DefaultTransactionScope())
            {

                using (var context = CreateEventContext())
                {
                    var existingSnapshot = context.Snapshots.Where(s => s.SnapshotId == aggregateId).FirstOrDefault();

                    if (existingSnapshot == null)
                    {
                        context.Snapshots.Add(new Snapshot()
                        {
                            SnapshotId = aggregateId,
                            CreatedBy = $"{Environment.UserDomainName}\\{Environment.UserName}",
                            CreatedUtc = DateTime.UtcNow,
                            SnapshotType = aggregate.GetType().FullName,
                            SnapshotData = snapshotSerialization.SerializeSnapshot(aggregate),
                            Version = aggregateVersion
                        });
                    }
                    else
                    {
                        existingSnapshot.CreatedBy = $"{Environment.UserDomainName}\\{Environment.UserName}";
                        existingSnapshot.CreatedUtc = DateTime.UtcNow;
                        existingSnapshot.SnapshotType = aggregate.GetType().FullName;
                        existingSnapshot.SnapshotData = snapshotSerialization.SerializeSnapshot(aggregate);
                        existingSnapshot.Version = aggregateVersion;
                    }

                    context.SaveChanges();
                }

                transactionScope.Complete();
            }
        }

        private Event ConvertEvent(string streamId, IEvent item)
        {
            var eventData = this.eventSerialization.SerializeEvent(item.EventBody);

            var userName = $"{Environment.UserDomainName}\\{Environment.UserName}";

            var storedEvent = new Event
            {
                StreamId = streamId,
                EventId = item.Id,
                StreamType = item.AggregateType,
                Version = item.AggregateVersion,
                CommandId = item.CommandId,
                CorrelationId = item.CorrelationId,
                EventType = item.EventBody.GetType().FullName,
                EventData = eventData,
                CreatedBy = userName,
                CreatedUtc = DateTime.UtcNow
            };

            return storedEvent;
        }

        private EventContext CreateEventContext()
        {
            return new EventContext(connectionString, dbSchema, eventTableName, snapshotTableName);
        }

        private static void GuardStreamConcurency(IEvent nextEvent, int expectedNextVersion)
        {
            if (nextEvent.AggregateVersion != expectedNextVersion)
            {
                throw new EventStreamConcurrencyException(nextEvent.AggregateId, nextEvent.AggregateVersion, expectedNextVersion);
            }
        }

        private static void GuardEventArgregateTypes(IEnumerable<IEvent> events, string expectedAggregateType)
        {
            if (!events.All(e => e.AggregateType == expectedAggregateType))
            {
                var unexpectedEvent = events.First(e => e.AggregateType != expectedAggregateType);

                throw new ArgumentOutOfRangeException(nameof(events), $"Event with type {unexpectedEvent.AggregateType} encountered but stream already associated with type {expectedAggregateType}.");
            }
        }

        private static void GuardEventVersions(IEnumerable<IEvent> events, int expectedNextVersion)
        {
            if (!events.Select(e => e.AggregateVersion).SequenceEqual(Enumerable.Range(expectedNextVersion, events.Count())))
            {
                throw new ArgumentOutOfRangeException(nameof(events), "Events must have contiguous versions.");
            }
        }

        private class Snapshot<TAggregate> : ISnapshot<TAggregate>
        {
            public TAggregate Aggregate { get; set; }

            public string SnapshotId { get; set; }

            public int Version { get; set; }
        }
    }
}
