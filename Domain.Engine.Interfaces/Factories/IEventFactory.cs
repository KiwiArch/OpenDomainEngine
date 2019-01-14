namespace Ode.Domain.Engine.Factories
{
    public interface IEventFactory
    {
        IEvent<T> CreateEvent<T>(string aggregateId, int aggregateVersion, string aggregateType, string commandId, string correlationId, T domainEvent);

        IEvent<TEvent> CreateEvent<TAggregate, TEvent>(string aggregateId, int aggregateVersion, string commandId, string correlationId, TEvent domainEvent);
    }
}
