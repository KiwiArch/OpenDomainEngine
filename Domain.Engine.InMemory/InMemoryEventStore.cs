namespace Ode.Domain.Engine.InMemory.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Engine.Repositories;

    public class InMemoryEventStore : IEventStore
    {
        private class StoredEvent
        {
            public string StreamId { get; set; }

            public IEvent Event { get; set; }
        }

        private readonly Collection<StoredEvent> storedEvents = new Collection<StoredEvent>();

        public IEnumerable<IEvent> RetrieveById(string streamId)
        {
            return this.RetrieveById(streamId, 0);
        }

        public IEnumerable<IEvent> RetrieveById(string streamId, int fromVersion)
        {
            return this.storedEvents.Where(e => e.StreamId == streamId & e.Event.AggregateVersion > fromVersion).Select(e => e.Event).OrderBy(e => e.AggregateVersion);
        }

        public void Store(string streamId, IEnumerable<IEvent> events)
        {
            events.ToList().ForEach(e => this.storedEvents.Add(new StoredEvent { StreamId = streamId, Event = e }));
        }
    }
}
