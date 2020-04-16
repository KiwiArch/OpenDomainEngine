using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ode.Domain.Engine.SampleModel.Locations;
using Ode.Domain.Engine.SampleModel.Movements;
using Ode.Domain.Engine.Model;
using Ode.Domain.Engine.Model.Configuration;
using Moq;

namespace Ode.Domain.Engine.Tests
{
    [TestClass()]
    public class ContextMapTests
    {
        public class CommandMock
        {

        }

        public class EventMock
        {

        }

        public ContextMapTests()
        {

        }

        [TestMethod()]
        public void CreateContextMapTest()
        {
            Assert.IsInstanceOfType(new BoundedContextModel(), typeof(IResolveEventTypes));
        }

        [TestMethod]
        public void MapAppDomainTest()
        {
            var contextMap = new BoundedContextModel();

            contextMap.WithAllAppDomainAssemblies();
        }

        [TestMethod]
        public void MapFindsAggregateTest()
        {
            var contextMap = new BoundedContextModel();

            contextMap.WithAssemblyContaining<Movement>();

            var aggregateType = (contextMap as IBoundedContextModel).GetAggregateType(typeof(CreateLocation));

            Assert.IsNotNull(aggregateType);
            Assert.IsTrue(aggregateType == typeof(Location));
        }

        [TestMethod]
        public void UnknowCommandThrowsExceptionTest()
        {
            var contextMap = new BoundedContextModel();

            contextMap.WithAssembly(Assembly.GetExecutingAssembly().FullName);

            Assert.ThrowsException<ArgumentException>(() => (contextMap as IBoundedContextModel).GetAggregateType(typeof(CommandMock)));
        }

        [TestMethod]
        public void MapFindsProcessTest()
        {
            var contextMap = new BoundedContextModel();

            contextMap.WithAssemblyContaining<Location>();

            Assert.IsTrue(contextMap.IsEventHandlerType(typeof(Movement)));
        }

        [TestMethod]
        public void MapCommandTest()
        {
            var contextMap = new BoundedContextModel();

            contextMap.WithAssemblyContaining<Location>();

            Assert.IsTrue(contextMap.IsCommandType(typeof(CreateLocation)));
            Assert.IsFalse(contextMap.IsEventType(typeof(CreateLocation)));
        }

        [TestMethod]
        public void MapEventTest()
        {
            var contextMap = new BoundedContextModel();

            contextMap.WithAssemblyContaining<Location>();

            Assert.IsTrue(contextMap.IsEventType(typeof(LocationCreated)));
            Assert.IsFalse(contextMap.IsCommandType(typeof(LocationCreated)));
        }

        [TestMethod]
        public void WithDomainObjectResolverTest()
        {
            var resolver = new Mock<IDomainObjectResolver>();
            resolver.Setup(x => x.New<Location>()).Returns(new Location());

            var context = new BoundedContextModel().WithDomainObjectResolver(resolver.Object);

            context.DomainObjectResolver.New<Location>();

            resolver.Verify(x => x.New<Location>());
        }
    }
}