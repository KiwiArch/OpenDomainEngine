namespace Ode.Domain.Engine.Dispatchers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using Transactions;

    public class EventDispatcher : IEventDispatcher
    {
        private readonly IBoundedContextModel boundedContextModel;
        private readonly IEventHandler eventHandler;

        public EventDispatcher(IBoundedContextModel boundedContextModel, IEventHandler eventHandler)
        {
            this.boundedContextModel = boundedContextModel;
            this.eventHandler = eventHandler;
        }

        public void DispatchEvent(IEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            using (var transactionScope = DomainTransaction.DefaultTransactionScope())
            {
                this.DispatchFullyTypedEvent(domainEvent.AsFullyTypedEvent(domainEvent.EventBody));

                transactionScope.Complete();
            }
        }

        public void DispatchEvents(IEnumerable<IEvent> domainEvents)
        {
            if (domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            using (var transactionScope = DomainTransaction.DefaultTransactionScope())
            {
                domainEvents.ToList().ForEach(e => this.DispatchFullyTypedEvent(e.AsFullyTypedEvent(e.EventBody)));         
               
                transactionScope.Complete();
            }
        }

        private void DispatchFullyTypedEvent<TEvent>(IEvent<TEvent> @event)
        {
            var customEventHandlers = this.boundedContextModel.GetEventHandlerTypes(@event.EventBody.GetType());

            foreach (var eventHandlerType in customEventHandlers)
            {
                var eventHandlerId = this.boundedContextModel.EventHandlerModel(eventHandlerType).GetEventHandlerId(@event);

                this.eventHandler.Handle(@event, eventHandlerId, eventHandlerType);
            }
        }
    }
}
