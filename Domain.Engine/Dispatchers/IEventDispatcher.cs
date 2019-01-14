namespace Ode.Domain.Engine.Dispatchers
{
    using System.Collections.Generic;

    public interface IEventDispatcher
    {
        void DispatchEvent(IEvent domainEvent);

        void DispatchEvents(IEnumerable<IEvent> domainEvents);
    }
}
