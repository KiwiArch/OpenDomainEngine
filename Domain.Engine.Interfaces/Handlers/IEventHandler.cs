namespace Ode.Domain.Engine
{
    using System;
    using System.Collections.Generic;

    public interface IEventHandler
    {
        IEnumerable<ICommand> Handle<TEvent>(IEvent<TEvent> domainEvent, string eventHandlerId, Type eventHandlerType);
    }
}