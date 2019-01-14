namespace Ode.Domain.Engine.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Factories;
    using Model;

    internal class AggregateAdapter<TAggregateRoot> : IAggregateAdapter<TAggregateRoot>
    {
        private readonly List<IEvent> uncommittedChanges = new List<IEvent>();
        private readonly HashSet<string> commandHistory = new HashSet<string>();
        private readonly IAggregateModel aggregateModel;
        private readonly TAggregateRoot aggregateRoot;
        private readonly IBoundedContextModel contextMap;

        private int version = 0;
        public string AggregateId { get; private set; }

        internal AggregateAdapter(IBoundedContextModel contextMap, TAggregateRoot aggregateRoot)
        {
            this.contextMap = contextMap;
            this.aggregateRoot = aggregateRoot;
            this.aggregateModel = contextMap.AggregateModel(typeof(TAggregateRoot));
        }

        internal AggregateAdapter(IBoundedContextModel contextMap, TAggregateRoot aggregateRoot, int version)
            :this(contextMap, aggregateRoot)
        {
            this.version = version;
        }

        public IAggregateModel AggregateModel
        {
            get
            {
                return this.aggregateModel;
            }
        }

        public IEnumerable<IEvent> UncommittedChanges
        {
            get
            {
                return this.uncommittedChanges;
            }
        }

        public TAggregateRoot AggregateRoot
        {
            get
            {
                return this.aggregateRoot;
            }
        }

        dynamic IAggregateAdapter.AggregateRoot
        {
            get
            {

                return this.aggregateRoot;
            }
        }

        public int Version
        {
            get
            {
                return this.version;
            }
        }

        public IAggregateAdapter<TAggregateRoot> WithId(string aggregateId)
        {
            this.AggregateId = aggregateId;
            return this;
        }

        public void ClearUncommittedChanges()
        {
            this.uncommittedChanges.Clear();
        }

        public void Rehydrate(IEnumerable<IEvent> events)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(this.AggregateId), "Configure must be used to set the aggregate Id before rehydration.");

            if (events == null)
            {
                throw new ArgumentNullException("events");
            }

            events.ToList().ForEach(e =>
            {
                if (e.AggregateId != this.AggregateId)
                {
                    throw new ArgumentException($"Event with aggregates id {e.AggregateId} does not match current aggregate Id of {this.AggregateId}!");
                }

                if (e.Id != this.NextEventId())
                {
                    throw new ArgumentException($"Event Id {e.Id} does not match next aggregate event Id of {this.NextEventId()}!");
                }

                this.DispatchEvent(e.EventBody);

                this.version++;

                this.commandHistory.Add(e.CommandId);
            });
        }

        public IEnumerable<IEvent> ProcessCommand(ICommand command)
        {
            var results = new List<IEvent>();

            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (!this.commandHistory.Contains(command.Id))
            {
                var stateChange = this.DispatchCommand(command);

                if (stateChange != null)
                {
                    Type typeInfo = stateChange.GetType();

                    /* if the return type is a recogised event then emit it as an event */
                    if (this.contextMap.IsEventType(typeInfo))
                    {
                        this.AddStateChangeToResultingEvents(command, results, stateChange);
                    }

                    /* if any members of the returned class are recognised events then emit them as seperate events */
                    /* this is primarily to support returning multiple events from a single aggregate call in a tuple like format */
                    typeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => contextMap.IsEventType(p.PropertyType))
                        .Select(p => p.GetValue(stateChange))
                        .Where(v => v != null)
                        .ToList()
                        .ForEach(s => this.AddStateChangeToResultingEvents(command, results, s));

                }
            }

            this.uncommittedChanges.AddRange(results);

            return results;
        }

        private void AddStateChangeToResultingEvents(ICommand command, List<IEvent> results, dynamic stateChange)
        {
            this.version++;

            results.Add(this.CreateEventAdapter(this.AggregateId, this.version, command.Id, command.CorrelationId, stateChange));
        }

        private dynamic DispatchCommand(ICommand command)
        {
            Type type = this.AggregateRoot.GetType();
            Type commandType = command.CommandBody.GetType();

            Debug.WriteLine(commandType.ToString());

            while (type != typeof(object))
            {
                var handler = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly)
                    .Where(m => m.Name == "When")
                    .Where(m => m.GetParameters().Length == 1)
                    .Where(m => m.GetParameters().First().ParameterType == commandType).SingleOrDefault();

                if (handler != null)
                {
                    try
                    {
                        var stateChange = handler.Invoke(this.AggregateRoot, new object[] { command.CommandBody }) as dynamic;

                        return stateChange;
                    }
                    catch (TargetInvocationException e)
                    {
                        if (this.contextMap.HasExceptionEvent(e))
                        {
                            return this.contextMap.GetExceptionEvent(e.InnerException);
                        }
                        else
                        {
                            throw e.InnerException;
                        }
                    }
                }

                type = type.BaseType;
            }

            return null;
        }

        private IEvent<T> CreateEventAdapter<T>(string aggregateId, int aggregateVersion, string commandId, string correlationId, T domainEvent)
        {
            return EventFactory.Default.CreateEvent(aggregateId, aggregateVersion, this.AggregateRoot.GetType().FullName, commandId, correlationId, domainEvent);
        }

        private void DispatchEvent<TDomainEvent>(TDomainEvent domainEvent)
        {
            Type type = this.AggregateRoot.GetType();
            Type eventType = domainEvent.GetType();

            while (type != typeof(object))
            {
                var handlers = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.DeclaredOnly)
                    .Where(m => m.Name == "Then")
                    .Where(m => m.GetParameters().Length == 1)
                    .Where(m => m.GetParameters().First().ParameterType == eventType).ToList();

                handlers.ForEach(handler => handler.Invoke(this.AggregateRoot, new object[] { domainEvent }));

                type = type.BaseType;
            }
        }

        private string NextEventId()
        {
            return $"{this.AggregateId}\\{this.version + 1}";
        }
    }
}


