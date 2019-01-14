namespace Ode.Domain.Engine.Brokers
{
    using System.Collections.Generic;

    public interface IEventBroker
    {
        IEnumerable<IEvent> BrokerEvent<TEvent>(IEvent<TEvent> domainEvent);
    }
}
