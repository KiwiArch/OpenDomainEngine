namespace Ode.Domain.Engine.Serialization
{
    public interface ISerializeEvents
    {
        string SerializeEvent<TEvent>(TEvent eventToSerialize);

        IEvent DeserializeEvent(string aggregateId, int aggregateVersion, string aggregateType, string commandId, string correlationId, string serializedEvent);
    }
}
