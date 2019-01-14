namespace Ode.Domain.Engine.Model
{
    using System.Collections.Generic;
    using CommandType = System.Type;
    using EventType = System.Type;


    internal class AggregateModel : IAggregateModel
    {
        private readonly Dictionary<CommandType, EventType> commandMap = new Dictionary<CommandType, EventType>();

        public bool IsSnapshotEnabled
        {
            get { return this.SnapshotFrequency > 0; }
        }

        public int SnapshotFrequency { get; set; }

        internal void AddCommandMap(CommandType commandType, CommandType eventType)
        {
            if (this.commandMap.ContainsKey(commandType))
            {
                this.commandMap[commandType] = eventType;
            }
            else
            {
                this.commandMap.Add(commandType, eventType);
            }
        }
    }
}
