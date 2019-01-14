namespace Ode.Domain.Engine.Repositories
{
    public interface ISnapshotRepository
    {
        void Store<TAggregate>(string aggregateId, int aggregateVersion, TAggregate aggregate);

        ISnapshot<TAggregate> Retrieve<TAggregate>(string id);
    }
}
