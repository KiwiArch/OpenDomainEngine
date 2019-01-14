namespace Ode.Domain.Engine.Model
{
    public interface IAggregateModel
    {
        bool IsSnapshotEnabled { get; }

        int SnapshotFrequency { get; }
    }
}
