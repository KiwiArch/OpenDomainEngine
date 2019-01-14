namespace Ode.Domain.Engine.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Factories;
    using Model;

    internal class EventHandlerAdapter<TEventHandler> : IEventHandlerAdapter<TEventHandler>
    {
        private readonly TEventHandler eventHandler;
        private readonly IBoundedContextModel contextMap;
        private readonly IEventHandlerModel eventHandlerModel;

        private ICommandFactory commandFactory;
        private List<ICommand> uncommittedCommands = new List<ICommand>();
        private List<IEvent> uncommittedEvents = new List<IEvent>();
        private HashSet<string> eventHistory = new HashSet<string>();

        private int version;

        public string EventHandlerId { get; protected set; }

        public IEnumerable<IEvent> UncommittedChanges
        {
            get
            {
                return this.uncommittedEvents;
            }
        }

        public EventHandlerAdapter(TEventHandler eventHandler, IBoundedContextModel contextMap)
        {
            this.eventHandler = eventHandler;
            this.contextMap = contextMap;
            this.eventHandlerModel = this.contextMap.EventHandlerModel(typeof(TEventHandler));
        }

        public IEventHandlerAdapter<TEventHandler> WithId(string eventHandlerId)
        {
            this.EventHandlerId = eventHandlerId;
            return this;
        }

        public IEventHandlerAdapter<TEventHandler> WithCommandFactory(ICommandFactory commandFactory)
        {
            this.commandFactory = commandFactory;
            return this;
        }

        public IEnumerable<ICommand> ProcessEvent(IEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            if (!this.eventHistory.Contains(domainEvent.Id))
            {
                var existingUncommitedChanges = new List<ICommand>(this.uncommittedCommands);

                this.DispatchEvent(domainEvent);

                this.eventHistory.Add(domainEvent.Id);

                var processedEvent = this.CreateProcessedEventAdapter(this.EventHandlerId, this.version, domainEvent as dynamic);
                this.uncommittedEvents.Add(processedEvent);

                return this.uncommittedCommands.Where(e => !existingUncommitedChanges.Contains(e)).ToList();
            }
            else
            {
                return new List<ICommand>();
            }
        }

        public void Rehydrate(IEnumerable<IEvent> events)
        {
            Debug.Assert(this.version == 0, "Process already hydrated.");
            Debug.Assert(!string.IsNullOrWhiteSpace(this.EventHandlerId), "Configure must be used to set the process Id before rehydration.");

            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            events.ToList().ForEach(e =>
            {
                this.DispatchEvent(e);
               
                this.eventHistory.Add(e.Id);

            });

            this.ClearUncommittedChanges();
        }

        public void ClearUncommittedChanges()
        {
            this.uncommittedCommands.Clear();
            this.uncommittedEvents.Clear();
        }

        private IEnumerable<ICommand> DispatchEvent(IEvent domainEvent)
        {
            var results = new List<ICommand>();

            Type type = this.eventHandler.GetType();
            Type eventType = domainEvent.EventBody.GetType();

            Debug.WriteLine(eventType.ToString());

            while (eventType != typeof(object))
            {
                var handlers = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly)
                    .Where(m => m.Name == "When")
                    .Where(m => m.GetParameters().Length == 1)
                    .Where(m => m.GetParameters().First().ParameterType == eventType).ToList();

                handlers.ForEach(handler =>
                {
                    var newCommand = handler.Invoke(this.eventHandler, new object[] { domainEvent.EventBody }) as dynamic;

                    this.version++;

                    if (newCommand != null)
                    {
                        var targetAggregateId = this.eventHandlerModel.GetCommandAggregateId(newCommand);

                        results.Add(this.CreateCommandAdapter(this.EventHandlerId, this.version, targetAggregateId, domainEvent.CorrelationId, newCommand));
                    }
                });

                eventType = eventType.BaseType;
            }

            this.uncommittedCommands.AddRange(results);

            return results;
        }

        private ICommand<T> CreateCommandAdapter<T>(string processId, int processStep, string aggregateId, string correlationId, T command)
        {
            return (this.commandFactory ?? CommandFactory.Default).CreateCommand<T>(processId, processStep, correlationId, aggregateId, command);
        }

        private IEvent<T> CreateProcessedEventAdapter<T>(string processId, int processStep, IEvent<T> domainEvent)
        {
            return new ProcessedEventAdapter<T>(processId, processStep, this.eventHandler.GetType(), domainEvent);
        }
    }
}

