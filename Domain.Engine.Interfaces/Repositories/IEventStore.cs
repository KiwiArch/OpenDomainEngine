namespace Ode.Domain.Engine.Repositories
{
    using System.Collections.Generic;

    public interface IReadEventStreams
    {
        IEnumerable<IEvent> RetrieveById(string streamId);

        IEnumerable<IEvent> RetrieveById(string streamId, int fromVersion);
    }

    public interface IStoreEventStreams
    {
        void Store(string streamId, IEnumerable<IEvent> events);
    }

    public interface IEventStore : IReadEventStreams, IStoreEventStreams
    {
    }
}
