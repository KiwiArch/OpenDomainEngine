namespace Ode.Domain.Engine.Factories
{
    using Adapters;
    using Model;

    internal class AggregateAdapterFactory : IAggregateAdapterFactory
    {
        private readonly static AggregateAdapterFactory defaultInstance = new AggregateAdapterFactory();

        public static AggregateAdapterFactory Default { get { return defaultInstance; } }

        public IAggregateAdapter<TAggregate> CreateAggregate<TAggregate>(IBoundedContextModel contextMap, TAggregate aggregate)
        {
            return new AggregateAdapter<TAggregate>(contextMap, aggregate);
        }

        public IAggregateAdapter<TAggregate> CreateAggregate<TAggregate>(IBoundedContextModel contextMap, TAggregate aggregate, int version)
        {
            return new AggregateAdapter<TAggregate>(contextMap, aggregate, version);
        }
    }
}
