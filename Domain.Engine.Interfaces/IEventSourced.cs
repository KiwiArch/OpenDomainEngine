namespace Ode.Domain.Engine
{
    using System.Collections.Generic;

    public interface IEventSourced
    {
        IEnumerable<IEvent> UncommittedChanges { get; }

        void ClearUncommittedChanges();

        void Rehydrate(IEnumerable<IEvent> events);
    }
}
