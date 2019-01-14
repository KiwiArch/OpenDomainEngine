namespace Ode.Domain.Engine.Model
{
    using System;

    public interface IBoundedContextModelSetup
    {
        IBoundedContextModel WithCommandHandler<TCommand, TAggregate, TEvent>();

        IBoundedContextModel WithEventHandler<TEvent, TEventHandler>(Func<IEvent<TEvent>, string> eventHandlerId);

        IBoundedContextModel WithEventHandler<TEvent, TEventHandler, TCommand>(Func<IEvent<TEvent>, string> eventHandlerId, Func<TCommand, string> aggregateId);

        IBoundedContextModel WithDomainObjectResolver(IDomainObjectResolver domainObjectResolver);

        IBoundedContextModel WithSnapshot<TAggregate>(int frequency);

        IBoundedContextModel WithExceptionEvent<TEvent>(Func<Exception, TEvent> createExceptionEvent);

        IBoundedContextModel WithExceptionEvent<TException, TEvent>(Func<TException, TEvent> createExceptionEvent) where TException : Exception;
    }
}
