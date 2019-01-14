namespace Ode.Domain.Engine.Adapters
{
    using System.Collections.Generic;

    internal interface IEventHandlerAdapter : IEventSourced
    {
        string EventHandlerId { get; }

        IEnumerable<ICommand> ProcessEvent(IEvent domainEvent);
    }

    internal interface IEventHandlerAdapter<TProcess> : IEventHandlerAdapter
    {
        IEventHandlerAdapter<TProcess> WithId(string processId);
    }
}