namespace Ode.Domain.Engine
{
    using System;
    using System.Collections.Generic;
    using Model;

    internal class EventHandler : IEventHandler
    {
        private readonly IBoundedContextModel contextModel;
        private readonly IRuntimeModel runtimeModel; 

        public EventHandler(IBoundedContextModel contextModel, IRuntimeModel runtimeModel)
        {
            this.contextModel = contextModel;
            this.runtimeModel = runtimeModel;
        }

        public IEnumerable<ICommand> Handle<TEvent>(IEvent<TEvent> domainEvent, string eventHandlerId, Type eventHandlerType)
        {
            var defaultInstance = this.contextModel.DomainObjectResolver.New(eventHandlerType) as dynamic;

            var eventHandlerAdapter = this.runtimeModel.EventHandlers.RetrieveById(eventHandlerId, defaultInstance);

            var results = eventHandlerAdapter.ProcessEvent(domainEvent);

            this.runtimeModel.Store();

            return results;
        }
    }
}
