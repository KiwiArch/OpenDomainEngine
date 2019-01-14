namespace Ode.Domain.Engine.Adapters
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class CommandAdapter<TCommand> : ICommand<TCommand>
    {
        public CommandAdapter(string commandId, string aggregateId, TCommand command)
            : this(commandId: commandId, correlationId: commandId, aggregateId: aggregateId, command: command)
        {
        }

        public CommandAdapter(string processId, int processStep, string correlationId, string aggregateId, TCommand command)
            : this(commandId: $"{processId}\\{processStep}", correlationId: correlationId, aggregateId: aggregateId, command: command)
        {
        }

        public CommandAdapter(string commandId, string correlationId, string aggregateId, TCommand command)
        {
            this.Id = commandId;
            this.AggregateId = aggregateId;
            this.CorrelationId = correlationId;
            this.CommandBody = command;
            this.CommandBodyType = typeof(TCommand).FullName;
        }

        public string Id { get; private set; }

        public string CorrelationId { get; private set; }

        public string AggregateId { get; private set; }

        public TCommand CommandBody { get; private set; }

        public string CommandBodyType { get; private set; }

        dynamic ICommand.CommandBody
        {
            get
            {
                return this.CommandBody;
            }
        }

        ICommand<T> ICommand.AsFullyTypedCommand<T>(T commandBody)
        {
            if (typeof(T) != typeof(TCommand))
            {
                throw new ArgumentException("Type mismatch, the type of the commandBody paramter does not match the type of this instances CommandBody property.", nameof(commandBody));
            }

            return this as ICommand<T>;
        }
    }
}
