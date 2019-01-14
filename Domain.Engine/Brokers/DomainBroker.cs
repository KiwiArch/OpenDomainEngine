namespace Ode.Domain.Engine.Brokers
{
    using System;
    using System.Collections.Generic;
    using Model;
    using Transactions;

    public class DomainBroker : IDomainBroker
    {
        private readonly IBoundedContextModel boundedContextModel;
        private readonly IEventHandler eventHandler;
        private readonly ICommandHandler commandHandler;

        public DomainBroker(IBoundedContextModel boundedContextModel, IEventHandler eventHandler, ICommandHandler commandHandler)
        {
            if (boundedContextModel == null)
            {
                throw new ArgumentNullException(nameof(boundedContextModel));
            }

            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            if (commandHandler == null)
            {
                throw new ArgumentNullException(nameof(commandHandler));
            }

            this.boundedContextModel = boundedContextModel;
            this.eventHandler = eventHandler;
            this.commandHandler = commandHandler;
        }

        public IEnumerable<IEvent> BrokerCommand<TCommand>(ICommand<TCommand> command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var results = new List<IEvent>();

            using (var transactionScope = DomainTransaction.DefaultTransactionScope())
            {
                var events = this.commandHandler.HandleCommand(command, command.AggregateId, this.boundedContextModel.GetAggregateType(command.CommandBody.GetType()));

                foreach (var @event in events)
                {
                    results.Add(@event);
                    results.AddRange(this.BrokerEvent(@event.AsFullyTypedEvent(@event.EventBody)));
                }

                transactionScope.Complete();
            }

            return results;
        }

        public IEnumerable<IEvent> BrokerEvent<TEvent>(IEvent<TEvent> domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            var results = new List<IEvent>();

            using (var transactionScope = DomainTransaction.DefaultTransactionScope())
            {
                var customEventHandlers = this.boundedContextModel.GetEventHandlerTypes(domainEvent.EventBody.GetType());

                var commands = new List<ICommand>();

                foreach (var eventHandlerType in customEventHandlers)
                {
                    var eventHandlerId = this.boundedContextModel.EventHandlerModel(eventHandlerType).GetEventHandlerId(domainEvent);

                    commands.AddRange(this.eventHandler.Handle(domainEvent, eventHandlerId, eventHandlerType));
                }

                foreach (var command in commands)
                {
                    results.AddRange(this.BrokerCommand(command.AsFullyTypedCommand(command.CommandBody)));
                }

                transactionScope.Complete();
            }

            return results;
        }
    }
}
