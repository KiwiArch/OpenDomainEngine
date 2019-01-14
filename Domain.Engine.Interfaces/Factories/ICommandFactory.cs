namespace Ode.Domain.Engine.Factories
{
    public interface ICommandFactory
    {
        ICommand<TCommand> CreateCommand<TCommand>(string id, string correlationId, string aggregateId, TCommand command);

        ICommand<TCommand> CreateCommand<TCommand>(string id, string aggregateId, TCommand command);

        ICommand<TCommand> CreateCommand<TCommand>(string processId, int step, string correlationId, string aggregateId, TCommand command);
    }
}
