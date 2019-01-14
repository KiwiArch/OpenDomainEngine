namespace Ode.Domain.Engine.Model
{
    using Repositories;

    internal sealed class RuntimeModel : IRuntimeModel
    {
        private readonly RuntimeAggregateModel aggregates;
        private readonly RuntimeEventHandlerModel eventHandlers;

        public RuntimeModel(IBoundedContextModel contextMap, IEventStore eventStore)
        {
            this.aggregates = new RuntimeAggregateModel(contextMap, eventStore);
            this.eventHandlers = new RuntimeEventHandlerModel(contextMap, eventStore);
        }

        public IRuntimeAggregateModel Aggregates { get { return this.aggregates; } }

        public IEventHandlerRuntimeModel EventHandlers { get { return this.eventHandlers; } }

        void IRuntimeModel.Clear()
        {
            this.aggregates.Clear();
            this.eventHandlers.Clear();
        }

        void IRuntimeModel.Store()
        {
            this.aggregates.Store();
            this.eventHandlers.Store();
        }
    }
}

