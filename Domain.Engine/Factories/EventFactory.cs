namespace Ode.Domain.Engine.Factories
{
    using Adapters;

    public class EventFactory : IEventFactory
    {
        private readonly static EventFactory defaultInstance = new EventFactory();

        public static EventFactory Default { get { return defaultInstance; } }

        public IEvent<TEvent> CreateEvent<TEvent>(string aggregateId, int aggregateVersion, string aggregateType, string commandId, string correlationId, TEvent domainEvent)
        {
            return new EventAdapter<TEvent>(aggregateId, aggregateVersion, aggregateType, commandId, correlationId, domainEvent);
        }

        public IEvent<TEvent> CreateEvent<TAggregate, TEvent>(string aggregateId, int aggregateVersion, string commandId, string correlationId, TEvent domainEvent)
        {
            return new EventAdapter<TEvent>(aggregateId, aggregateVersion, typeof(TAggregate).FullName, commandId, correlationId, domainEvent);
        }
    }
}
