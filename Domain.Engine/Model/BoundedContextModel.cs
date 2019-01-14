namespace Ode.Domain.Engine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using AggregateType = System.Type;
    using CommandType = System.Type;
    using EventHandlerType = System.Type;
    using EventType = System.Type;
    using ExceptionType = System.Type;

    internal sealed class BoundedContextModel : IBoundedContextModel
    {
        private readonly IDictionary<CommandType, AggregateType> commandMap = new Dictionary<CommandType, AggregateType>();
        private readonly IDictionary<AggregateType, AggregateModel> aggregates = new Dictionary<AggregateType, AggregateModel>();

        private readonly IDictionary<EventType, ICollection<EventHandlerType>> eventMap = new Dictionary<EventType, ICollection<EventHandlerType>>();
        private readonly IDictionary<EventHandlerType, EventHandlerModel> eventHandlers = new Dictionary<EventHandlerType, EventHandlerModel>();
        
        private IDomainObjectResolver domainObjectResolver = new DomainObjectResolver();

        private readonly IDictionary<ExceptionType, Delegate> exceptionMap = new Dictionary<ExceptionType, Delegate>();  // Func<Exception, dynamic> exceptionEventFactoryFunction;

        public BoundedContextModel()
        {
        }

        public IDomainObjectResolver DomainObjectResolver
        {
            get { return this.domainObjectResolver; }
        }

        public IAggregateModel AggregateModel(AggregateType aggregateType)
        {
            return this.aggregates[aggregateType];
        }

        public IEventHandlerModel EventHandlerModel(EventHandlerType eventHandlerType)
        {
            return this.eventHandlers[eventHandlerType];
        }

        public IBoundedContextModel WithCommandHandler<TCommand, TAggregate, TEvent>()
        {
            var aggregateType = typeof(TAggregate);

            if (!this.aggregates.ContainsKey(aggregateType))
            {
                this.aggregates.Add(aggregateType, new AggregateModel());
            }

            var commandType = typeof(TCommand);
            var eventType = typeof(TEvent);

            this.aggregates[aggregateType].AddCommandMap(commandType, eventType);

            Trace.WriteLine($"Mapped command type {commandType} to aggregate type {aggregateType}");

            if (!this.commandMap.ContainsKey(commandType))
            {
                this.commandMap.Add(commandType, aggregateType);
            }

            this.AddToEventMap(eventType, null);

            return this;
        }

        public IBoundedContextModel WithSnapshot<TAggregate>(int frequency)
        {
            var aggregateType = typeof(TAggregate);

            if (!this.aggregates.ContainsKey(aggregateType))
            {
                this.aggregates.Add(aggregateType, new AggregateModel());
            }

            this.aggregates[aggregateType].SnapshotFrequency = frequency;

            return this;
        }

        public IBoundedContextModel WithEventHandler<TEvent, TEventHandler>(Func<IEvent<TEvent>, string> eventHandlerId)
        {
            if (eventHandlerId == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerId));
            }

            var eventType = typeof(TEvent);
            var eventHandlerType = typeof(TEventHandler);

            if (!this.eventHandlers.ContainsKey(eventHandlerType))
            {
                this.eventHandlers.Add(eventHandlerType, new EventHandlerModel(eventHandlerType));
            }

            this.eventHandlers[eventHandlerType].AddEventHandlerIdDelegate(eventHandlerId);

            this.AddToEventMap(eventType, eventHandlerType);

            return this;
        }

        public IBoundedContextModel WithEventHandler<TEvent, TEventHandler, TCommand>(Func<IEvent<TEvent>, string> eventHandlerId, Func<TCommand, string> aggregateId)
        {
            if (aggregateId == null)
            {
                throw new ArgumentNullException(nameof(aggregateId));
            }

            this.WithEventHandler<TEvent, TEventHandler>(eventHandlerId);

            this.eventHandlers[typeof(TEventHandler)].AddCommandIdDelegate(aggregateId);

            return this;
        }

        public IBoundedContextModel WithDomainObjectResolver(IDomainObjectResolver domainObjectResolver)
        {
            this.domainObjectResolver = domainObjectResolver;
            return this;
        }

        public Type GetAggregateType(Type commandType)
        {
            GuardCommandType(commandType);

            return this.commandMap[commandType];
        }

        public IEnumerable<Type> GetEventHandlerTypes(Type eventType)
        {
            return this.AppendEventHandlerTypes(eventType, new List<Type>());

            //if (!this.eventMap.ContainsKey(eventType))
            //{
            //    Trace.TraceInformation($"{eventType} is not a mapped event.");

            //    return new Type[0];
            //}
            //else
            //{
            //    return this.eventMap[eventType];
            //}
        }

        private List<Type> AppendEventHandlerTypes(Type eventType, List<Type> results)
        {
            if (this.eventMap.ContainsKey(eventType))
            {
                results.AddRange(this.eventMap[eventType]);
            }

            if (eventType.BaseType != typeof(System.Object))
            {
                this.AppendEventHandlerTypes(eventType.BaseType, results);
            }

            return results;
        }

        public bool IsEventType(Type type)
        {
            return this.eventMap.ContainsKey(type);
        }

        public bool IsCommandType(Type type)
        {
            return this.commandMap.ContainsKey(type);
        }

        public bool IsAggregateType(Type type)
        {
            return this.commandMap.Values.Any(t => t == type);
        }

        public bool IsEventHandlerType(Type type)
        {
            return this.eventMap.Values.Any(c => c.Any(t => t == type));
        }

        public Type ResolveEventType(string eventTypeFullName)
        {
            return this.eventMap.Keys.Single(t => t.Name == eventTypeFullName || t.FullName == eventTypeFullName || t.AssemblyQualifiedName == eventTypeFullName);
        }

        public Type ResolveCommandType(string commandTypeFullName)
        {
            return this.commandMap.Keys.Single(t => t.FullName == commandTypeFullName);
        }

        public string ResolveEventTypeFullName(Type eventType)
        {
            if (eventType == null)
            {
                throw new ArgumentNullException(nameof(eventType));
            }

            return eventType.FullName;
        }

        public string ResolveCommandTypeFullName(Type commandType)
        {
            if (commandType == null)
            {
                throw new ArgumentNullException(nameof(commandType));
            }

            return commandType.FullName;
        }

        public bool HasExceptionEvent<TException>(TException exception)
        {
            var exceptionType = exception.GetType();

            do
            {
                if (this.exceptionMap.ContainsKey(exceptionType))
                {
                    return true;
                }

                exceptionType = exceptionType.BaseType;
            }
            while (exceptionType != null);

            return false;
        }

        public dynamic GetExceptionEvent<TException>(TException exception) where TException : Exception
        {
            var exceptionType = exception.GetType();

            do
            {
                if (this.exceptionMap.ContainsKey(exceptionType))
                {
                    return (this.exceptionMap[exceptionType] as Func<TException, dynamic>)(exception);
                }

                exceptionType = exceptionType.BaseType;
            }
            while (exceptionType != null);

            return null;
        }

        public IBoundedContextModel WithExceptionEvent<TEvent>(Func<Exception, TEvent> createExceptionEvent)
        {
            return this.WithExceptionEvent<Exception, TEvent>(createExceptionEvent);
        }
        
        public IBoundedContextModel WithExceptionEvent<TException, TEvent>(Func<TException, TEvent> createExceptionEvent) where TException : Exception
        {
            this.AddToEventMap(typeof(TEvent), null);

            var exceptionType = typeof(TException);

            if (this.exceptionMap.ContainsKey(exceptionType))
            {
                this.exceptionMap[exceptionType] = createExceptionEvent;
            }
            else
            {
                this.exceptionMap.Add(exceptionType, createExceptionEvent);
            }

            return this;
        }

        private void GuardCommandType(Type commandType)
        {
            if (!this.commandMap.ContainsKey(commandType))
            {
                throw new ArgumentException($"{commandType} is not a mapped command.");
            }
        }

        private void AddToEventMap(Type eventType, Type eventHandlerType)
        {
            if (!this.eventMap.ContainsKey(eventType))
            {
                this.eventMap.Add(eventType, new Collection<Type>());
            }

            if (eventHandlerType != null && !this.eventMap[eventType].Contains(eventHandlerType))
            {
                this.eventMap[eventType].Add(eventHandlerType);

                Trace.WriteLine($"Mapped event type {eventType} to hander type {eventHandlerType}");
            }
        }
    }
}
