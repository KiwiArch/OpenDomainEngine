namespace Ode.Domain.Engine.Model
{
    using System;
    using System.Collections.Generic;

    public interface IBoundedContextModel : IResolveEventTypes, IResolveCommandTypes, IBoundedContextModelSetup
    {
        Type GetAggregateType(Type commandType);

        IEnumerable<Type> GetEventHandlerTypes(Type eventType);

        bool HasExceptionEvent<TException>(TException exception);

        IDomainObjectResolver DomainObjectResolver { get; }
        
        bool IsEventType(Type type);

        bool IsCommandType(Type type);

        bool IsAggregateType(Type type);

        bool IsEventHandlerType(Type type);

        //bool IsSnapshotEnabled(Type aggregateType);

        //int SnapshotFrequency(Type aggregateType);

        dynamic GetExceptionEvent<TException>(TException e) where TException : Exception;

        IAggregateModel AggregateModel(Type aggregateType);

        IEventHandlerModel EventHandlerModel(Type eventHandlerType);
    }
}
