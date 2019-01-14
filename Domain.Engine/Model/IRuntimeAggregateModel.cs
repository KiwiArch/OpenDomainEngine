namespace Ode.Domain.Engine.Model
{
    using Adapters;

    internal interface IRuntimeAggregateModel
    {
        IAggregateAdapter<TAggregate> Create<TAggregate>(TAggregate aggregate, string id);

        IAggregateAdapter<TAggregate> RetrieveById<TAggregate>(string id, TAggregate defaultInstance = null) where TAggregate : class;
    }
}
