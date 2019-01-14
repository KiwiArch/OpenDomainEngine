namespace Ode.Domain.Engine.Repositories
{
    public interface ISnapshot<TAggregate>
    {
        string SnapshotId { get; }

        TAggregate Aggregate { get; }

        int Version { get; }
    }
}
