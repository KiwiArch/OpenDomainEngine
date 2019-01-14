namespace Ode.Domain.Engine
{
    public interface ICommand : IDomainMessage
    {
        string AggregateId { get; }

        dynamic CommandBody { get; }

        string CommandBodyType { get; }

        ICommand<T> AsFullyTypedCommand<T>(T commandBody);
    }

    public interface ICommand<TCommand> : ICommand
    {
        new TCommand CommandBody { get; }
    }
}
