namespace Ode.Domain.Engine.Adapters
{
    using System;

    internal class EventAdapter<TEvent> : IEvent<TEvent>
    {
        internal EventAdapter(string aggregateId, int aggregateVersion, string aggregateType, string commandId, string correlationId, TEvent domainEvent)
        {
            this.Id = $"{aggregateId}\\{aggregateVersion}";
            this.AggregateId = aggregateId;
            this.AggregateVersion = aggregateVersion;
            this.AggregateType = aggregateType;
            this.CommandId = commandId;
            this.CorrelationId = correlationId;
            this.EventBody = domainEvent;
            this.EventBodyType = typeof(TEvent).FullName;
        }

        public string Id { get; private set; }

        public string CommandId { get; private set; }

        public string CorrelationId { get; private set; }

        public TEvent EventBody { get; private set; }

        public string AggregateId { get; private set; }

        public string AggregateType { get; private set; }

        public int AggregateVersion { get; private set; }

        public string EventBodyType { get; private set; }

        dynamic IEvent.EventBody { get { return this.EventBody; } }

        public IEvent<T> AsFullyTypedEvent<T>(T eventBody)
        {
            if (typeof(T) != typeof(TEvent))
            {
                throw new ArgumentException("Type mismatch, the type of the eventBody paramter does not match the type of this instances EventBody property.", nameof(eventBody));
            }

            return this as IEvent<T>;
        }
    }
}
