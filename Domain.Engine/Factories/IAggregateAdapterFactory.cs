namespace Ode.Domain.Engine.Factories
{
    using Adapters;
    using Model;

    internal interface IAggregateAdapterFactory
    {
        IAggregateAdapter<TAggregate> CreateAggregate<TAggregate>(IBoundedContextModel contextMap, TAggregate aggregate);

        IAggregateAdapter<TAggregate> CreateAggregate<TAggregate>(IBoundedContextModel contextMap, TAggregate aggregate, int version);
    }
}
