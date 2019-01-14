namespace Ode.Domain.Engine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Factories;
    using Repositories;
    using Ode.Domain.Engine.Adapters;


    internal class RuntimeEventHandlerModel : IEventHandlerRuntimeModel
    {
        private readonly IDictionary<string, IEventHandlerAdapter> eventHandlerModel = new Dictionary<string, IEventHandlerAdapter>();
        private readonly IBoundedContextModel contextMap;
        private readonly IEventStore eventStore;

        public RuntimeEventHandlerModel(IBoundedContextModel contextMap, IEventStore eventStore)
        {
            this.contextMap = contextMap;
            this.eventStore = eventStore;
        }

        IEventHandlerAdapter<TEventHandler> IEventHandlerRuntimeModel.Create<TEventHandler>(TEventHandler process, string id)
        {
            var processAdapter = EventHandlerAdapterFactory.Default.CreateEventHandler<TEventHandler>(id, process, this.contextMap);

            this.eventHandlerModel.Add(id, processAdapter);

            return processAdapter;
        }

        internal void Clear()
        {
            this.eventHandlerModel.Clear();
        }

        internal void Store()
        {
            foreach (var item in this.eventHandlerModel.Values.Where(eh => eh.UncommittedChanges.Any()))
            {
                this.eventStore.Store(item.EventHandlerId, item.UncommittedChanges.ToList());
                item.ClearUncommittedChanges();
            }
        }

        IEventHandlerAdapter<TProcess> IEventHandlerRuntimeModel.RetrieveById<TProcess>(string id, TProcess defaultInstance)
        {
            var result = this.eventHandlerModel.ContainsKey(id) ? this.eventHandlerModel[id] as EventHandlerAdapter<TProcess> : this.RehydrateProcessFromBackingStore<TProcess>(id);

            return result;
        }

        private IEventHandlerAdapter<TEventHandler> RehydrateProcessFromBackingStore<TEventHandler>(string id)
        {
            var eventHandlerAdapter = EventHandlerAdapterFactory.Default.CreateEventHandler(id, this.contextMap.DomainObjectResolver.New<TEventHandler>(), this.contextMap);

            var events = this.eventStore.RetrieveById(id);

            if (events != null)
            {
                eventHandlerAdapter.Rehydrate(events);
            }

            this.eventHandlerModel.Add(id, eventHandlerAdapter);

            return eventHandlerAdapter;
        }
    }
}
