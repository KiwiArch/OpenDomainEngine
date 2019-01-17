namespace Ode.Domain.Engine.JsonSerialization
{
    using System.Reflection;
    using Engine;
    using Factories;
    using Newtonsoft.Json;
    using Serialization;

    public class CommandSerialization : ISerializeCommands
    {
        private readonly JsonSerializerSettings jsonSerializationSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, SerializationBinder = new SerializationBinder(a => Assembly.Load(a)) };

        public ICommand DeserializeCommand(string id, string correlationId, string aggregateId, string jsonCommand)
        {
            var domainEvent = JsonConvert.DeserializeObject(jsonCommand, jsonSerializationSettings);

            var commandFactoryProxy = new CommandFactoryProxy();

            return commandFactoryProxy.GetType()
                 .GetMethod(nameof(CommandFactoryProxy.CreateCommand))
                 .MakeGenericMethod(domainEvent.GetType())
                 .Invoke(commandFactoryProxy, new object[] { id, correlationId, aggregateId, domainEvent }) as ICommand;
        }

        public string SerializeCommand<TCommand>(TCommand commandToSerialize)
        {
            return JsonConvert.SerializeObject(commandToSerialize, jsonSerializationSettings);
        }

        private class CommandFactoryProxy
        {
            public ICommand<TCommand> CreateCommand<TCommand>(string id, string correlationId, string aggregateId, TCommand command)
            {
                return CommandFactory.Default.CreateCommand(id, correlationId, aggregateId, command);
            }
        }
    }
}
