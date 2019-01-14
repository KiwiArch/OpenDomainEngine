namespace Ode.Domain.Engine
{
    public interface IEventHeader : IDomainMessage
    {
        string AggregateId { get; }

        string AggregateType { get; }

        int AggregateVersion { get; }

        string CommandId { get; }
    }

    public interface IEvent : IEventHeader
    {
        dynamic EventBody { get; }

        string EventBodyType { get; }

        IEvent<T> AsFullyTypedEvent<T>(T eventBody);
    }

    public interface IEvent<out TEvent> : IEvent
    {
        new TEvent EventBody { get; }
    }

}
