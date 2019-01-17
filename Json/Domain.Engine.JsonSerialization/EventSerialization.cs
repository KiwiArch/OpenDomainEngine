namespace Ode.Domain.Engine.JsonSerialization
{
    using System.Reflection;
    using Engine;
    using Factories;
    using Newtonsoft.Json;
    using Serialization;

    public class EventSerialization : ISerializeEvents
    {
        private readonly JsonSerializerSettings jsonSerializationSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, SerializationBinder = new SerializationBinder(a => Assembly.Load(a)) };

        public EventSerialization()
        {

        }

        public IEvent DeserializeEvent(string aggregateId, int aggregateVersion, string aggregateType, string commandId, string correlationId, string serializedEvent)
        {
            var domainEvent = JsonConvert.DeserializeObject(serializedEvent, this.jsonSerializationSettings);

            var eventFactoryProxy = new EventFactoryProxy();

            return eventFactoryProxy.GetType()
                 .GetMethod(nameof(EventFactoryProxy.CreateEvent))
                 .MakeGenericMethod(domainEvent.GetType())
                 .Invoke(eventFactoryProxy, new object[] { aggregateId, aggregateVersion, aggregateType, commandId, correlationId, domainEvent }) as IEvent;
        }

        public string SerializeEvent<TEvent>(TEvent eventToSerialize)
        {
            return JsonConvert.SerializeObject(eventToSerialize, this.jsonSerializationSettings);
        }
           
        private class EventFactoryProxy
        {
            public IEvent<TEvent> CreateEvent<TEvent>(string aggregateId, int aggregateVersion, string aggregateType, string commandId, string correlationId, TEvent domainEvent)
            {
                return EventFactory.Default.CreateEvent(aggregateId, aggregateVersion, aggregateType, commandId, correlationId, domainEvent);
            }
        }
    }
}
