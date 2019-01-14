namespace Ode.Domain.Engine.Serialization
{
    public interface ISerializeCommands
    {
        string SerializeCommand<TCommand>(TCommand commandToSerialize);

        ICommand DeserializeCommand(string id, string correlationId, string aggregateId, string serializedCommand);
    }
}
