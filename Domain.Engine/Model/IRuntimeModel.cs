namespace Ode.Domain.Engine.Model
{
    using System;

    internal interface IRuntimeModel 
    {
        IRuntimeAggregateModel Aggregates { get; }

        IEventHandlerRuntimeModel EventHandlers { get; }

        void Clear();

        void Store();


    }
}
