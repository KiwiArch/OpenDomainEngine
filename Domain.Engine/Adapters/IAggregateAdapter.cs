namespace Ode.Domain.Engine.Adapters
{
    using System.Collections.Generic;
    using Model;

    internal interface IAggregateAdapter : IEventSourced
    {
        string AggregateId { get; }

        IEnumerable<IEvent> ProcessCommand(ICommand command);

        object AggregateRoot { get; }

        int Version { get; }

        IAggregateModel AggregateModel { get; }
    }

    internal interface IAggregateAdapter<TAggregate> : IAggregateAdapter
    {
        IAggregateAdapter<TAggregate> WithId(string id);

        new TAggregate AggregateRoot { get; }
    }
}
