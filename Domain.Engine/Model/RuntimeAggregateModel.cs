namespace Ode.Domain.Engine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Adapters;
    using Factories;
    using Repositories;

    internal class RuntimeAggregateModel : IRuntimeAggregateModel
    {
        private readonly IDictionary<string, IAggregateAdapter> aggregateModel = new Dictionary<string, IAggregateAdapter>();
        private readonly IBoundedContextModel contextMap;
        private readonly IEventStore eventStore;
        private readonly ISnapshotRepository snapshotRepository;

        public RuntimeAggregateModel(IBoundedContextModel contextMap, IEventStore eventStore)
        {
            this.contextMap = contextMap;
            this.eventStore = eventStore;
            this.snapshotRepository = eventStore as ISnapshotRepository;
        }

        public void Clear()
        {
            this.aggregateModel.Clear();
        }

        public void Store()
        {
            foreach (var item in this.aggregateModel.Values.Where(ar => ar.UncommittedChanges.Any()).ToList())
            {
                if (this.snapshotRepository != null && item.AggregateModel.IsSnapshotEnabled && (item.Version % item.AggregateModel.SnapshotFrequency == 0))
                {
                    this.snapshotRepository.Store(item.AggregateId, item.Version, item.AggregateRoot as dynamic);
                }

                this.eventStore.Store(item.AggregateId, item.UncommittedChanges.ToList());

                item.ClearUncommittedChanges();
            }
        }

        IAggregateAdapter<TAggregate> IRuntimeAggregateModel.Create<TAggregate>(TAggregate aggregate, string id)
        {
            var aggregateAdapter = (AggregateAdapterFactory.Default).CreateAggregate<TAggregate>(this.contextMap, aggregate).WithId(id);

            this.aggregateModel.Add(id, aggregateAdapter);

            return aggregateAdapter;
        }

        IAggregateAdapter<TAggregate> IRuntimeAggregateModel.RetrieveById<TAggregate>(string id, TAggregate defaultInstance)
        {
            var result = this.aggregateModel.ContainsKey(id) ? this.aggregateModel[id] as AggregateAdapter<TAggregate> : this.RehydrateAggregateFromBackingStore<TAggregate>(id);

            if (result == null)
            {
                throw new ArgumentException($"Unable to retrieve an aggregate with id:{id} of type:{typeof(TAggregate)}, ensure all aggregate id's in the domain are unique.");
            }

            return result;
        }

        private IAggregateAdapter<TAggregate> RehydrateAggregateFromBackingStore<TAggregate>(string id)
        {
            var snapshot = this.snapshotRepository != null ? this.snapshotRepository.Retrieve<TAggregate>(id) : null;

            var aggregateAdapter = snapshot == null
                ? AggregateAdapterFactory.Default.CreateAggregate(this.contextMap, this.contextMap.DomainObjectResolver.New<TAggregate>()).WithId(id)
                : AggregateAdapterFactory.Default.CreateAggregate(this.contextMap, snapshot.Aggregate, snapshot.Version).WithId(id);

            var events = this.eventStore.RetrieveById(id, aggregateAdapter.Version);

            if (events != null && events.Count() > 0)
            {
                aggregateAdapter.Rehydrate(events);
            }

            this.aggregateModel.Add(id, aggregateAdapter);

            return aggregateAdapter;
        }
    }
}
