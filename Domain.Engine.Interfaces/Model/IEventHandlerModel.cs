namespace Ode.Domain.Engine.Model
{
    using System;

    public interface IEventHandlerModel
    {
        string GetCommandAggregateId<TCommand>(TCommand command);

        string GetEventHandlerId<TEvent>(IEvent<TEvent> domainEvent);
    }
}
