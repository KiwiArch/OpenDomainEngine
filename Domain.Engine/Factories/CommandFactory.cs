namespace Ode.Domain.Engine.Factories
{
    using System;
    using Adapters;

    public class CommandFactory : ICommandFactory
    {
        private readonly static CommandFactory defaultInstance = new CommandFactory();

        public static CommandFactory Default { get { return defaultInstance; } }

        public ICommand<TCommand> CreateCommand<TCommand>(string id, string correlationId, string aggregateId, TCommand command)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            correlationId = string.IsNullOrWhiteSpace(correlationId) ? aggregateId : correlationId;

            if (string.IsNullOrWhiteSpace(aggregateId))
            {
                throw new ArgumentNullException(nameof(aggregateId));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return new CommandAdapter<TCommand>(id, correlationId, aggregateId, command);
        }

        public ICommand<TCommand> CreateCommand<TCommand>(string id, string aggregateId, TCommand command)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(aggregateId))
            {
                throw new ArgumentNullException(nameof(aggregateId));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return new CommandAdapter<TCommand>(id, aggregateId, command);
        }

        public ICommand<TCommand> CreateCommand<TCommand>(string processId, int step, string correlationId, string aggregateId, TCommand command)
        {
            if (string.IsNullOrWhiteSpace(processId))
            {
                throw new ArgumentNullException(nameof(processId));
            }

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentNullException(nameof(correlationId));
            }

            if (string.IsNullOrWhiteSpace(aggregateId))
            {
                throw new ArgumentNullException(nameof(aggregateId));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }            

            return new CommandAdapter<TCommand>(processId, step, correlationId, aggregateId, command);
        }
    }
}
