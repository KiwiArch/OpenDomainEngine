namespace Ode.Domain.Engine
{
    using System;
    using System.Collections.Generic;
    using Model;
    using Repositories;
    using Transactions;

    public class TransactionalEventHandler : IEventHandler
    {
        private readonly IRuntimeModel runtimeModel;
        private readonly IEventHandler eventHandler;
        private readonly bool cacheRuntimeModel;

        internal TransactionalEventHandler(IBoundedContextModel boundedContextModel, IEventStore eventStore, bool cacheRuntimeModel)
        {
            this.runtimeModel = new RuntimeModel(boundedContextModel, eventStore);
            this.eventHandler = new EventHandler(boundedContextModel, runtimeModel);
            this.cacheRuntimeModel = cacheRuntimeModel;
        }

        public IEnumerable<ICommand> Handle<TEvent>(IEvent<TEvent> domainEvent, string eventHandlerId, Type eventHandlerType)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            if (string.IsNullOrWhiteSpace(eventHandlerId))
            {
                throw new ArgumentNullException(nameof(eventHandlerId));
            }

            if (eventHandlerType == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerType));
            }

            return this.ProcessEvent(domainEvent, eventHandlerId, eventHandlerType);
        }

        private IEnumerable<ICommand> ProcessEvent<TEvent>(IEvent<TEvent> domainEvent, string eventHandlerId, Type eventHandlerType)
        {
            IEnumerable<ICommand> resultingCommands;

            using (var transactionScope = DomainTransaction.DefaultTransactionScope())
            {
                resultingCommands = this.eventHandler.Handle(domainEvent, eventHandlerId, eventHandlerType);

                if (!this.cacheRuntimeModel)
                {
                    this.runtimeModel.Clear();
                }

                transactionScope.Complete();
            }

            return resultingCommands;
        }
    }
}
