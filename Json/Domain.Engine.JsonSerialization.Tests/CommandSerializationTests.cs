namespace Ode.Domain.Engine.JsonSerialization.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ode.Domain.Engine.SampleModel.Locations;
    
    [TestClass]
    public class CommandSerializationTests
    {
        [TestMethod()]
        public void CreateCommandAdapterTest()
        {
            string aggregateId = "Box";
            string aggregateType = typeof(Location).FullName;
            string commandId = Guid.NewGuid().ToString();
            string correlationId = commandId;

            CreateLocation createLocationCommand = new CreateLocation(aggregateId);

            string jsonCommand = new CommandSerialization().SerializeCommand(createLocationCommand);

            var commandAdapter = new CommandSerialization().DeserializeCommand(commandId, correlationId, aggregateId, jsonCommand);

            Assert.IsInstanceOfType(commandAdapter, typeof(ICommand));
            Assert.IsInstanceOfType(commandAdapter, typeof(ICommand<CreateLocation>));
        }
    }
}