namespace Ode.Domain.Engine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AggregateType = System.Type;
    using CommandType = System.Type;
    using EventType = System.Type;

    internal class EventHandlerModel : IEventHandlerModel
    {
        private readonly IDictionary<EventType, Delegate> eventHandlers = new Dictionary<EventType, Delegate>();
        private readonly IDictionary<CommandType, Delegate> commands = new Dictionary<AggregateType, Delegate>();
        private readonly Type eventHandlerType;

        internal EventHandlerModel(Type eventHandlerType)
        {
            if (eventHandlerType == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerType));
            }

            this.eventHandlerType = eventHandlerType;       
        }

        internal void AddEventHandlerIdDelegate<TEvent>(Func<IEvent<TEvent>, string> eventHandlerId)
        {
            var eventType = typeof(TEvent);

            if (this.eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] = eventHandlerId;
            }
            else
            {
                eventHandlers.Add(eventType, eventHandlerId);
            }
        }

        internal void AddCommandIdDelegate<TCommand>(Func<TCommand, string> aggregateId)
        {
            var commandType = typeof(TCommand);

            if (this.commands.ContainsKey(commandType))
            {
                this.commands[commandType] = aggregateId;
            }
            else
            {
                this.commands.Add(commandType, aggregateId);
            }
        }

        public string GetEventHandlerId<TEvent>(IEvent<TEvent> domainEvent)
        {
            var eventType = typeof(TEvent);

            if (this.eventHandlers.ContainsKey(eventType))
            {
                return $"{eventHandlerType.Name}\\{(this.eventHandlers[eventType] as Func<IEvent<TEvent>, string>)(domainEvent)}";
            }
            else
            {
                if (this.IsHandledEventType(eventType.BaseType))
                {
                    return this.GetEventHandlerId(domainEvent, FormatterServices.GetUninitializedObject(eventType.BaseType) as dynamic);
                }
            }

            throw new ArgumentException("Event does not have a mapped handler.", nameof(domainEvent));
        }

        public bool IsHandledEventType(EventType eventType)
        {
            return this.eventHandlers.ContainsKey(eventType);
        }

        public string GetCommandAggregateId<TCommand>(TCommand command)
        {
            return (this.commands[command.GetType()] as Func<TCommand, string>)(command);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "baseEvent", Justification = "Required for generic type casting.")]
        private string GetEventHandlerId<TEvent, TBaseEvent>(IEvent<TEvent> domainEvent, TBaseEvent baseEvent) where TBaseEvent : class
        {
            var eventType = typeof(TBaseEvent);

            if (this.eventHandlers.ContainsKey(eventType))
            {
                return $"{eventHandlerType.Name}\\{(this.eventHandlers[eventType] as Func<IEvent<TBaseEvent>, string>)(domainEvent as IEvent<TBaseEvent>)}";
            }
            else
            {
                if (this.IsHandledEventType(eventType.BaseType))
                {
                    return this.GetEventHandlerId(domainEvent, FormatterServices.GetUninitializedObject(eventType.BaseType) as dynamic);
                }
            }

            throw new ArgumentException("Event does not have a mapped handler.", nameof(domainEvent));
        }
    }
}
