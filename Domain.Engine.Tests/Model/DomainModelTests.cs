using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ode.Domain.Engine.Repositories;
using Ode.Domain.Engine.SampleModel.Locations;
using System.Security;

namespace Ode.Domain.Engine.Model.Tests
{
    [TestClass]
    public class DomainModelTests
    {
        [TestMethod]
        public void DomainModelTest()
        {
            var domainModel = new RuntimeModel(new BoundedContextModel(), new Mock<IEventStore>().Object);

            Assert.IsInstanceOfType(domainModel, typeof(IRuntimeModel));
        }

        [TestMethod]
        public void WithDomainObjectResolverTest()
        {
            var resolver = new Mock<IDomainObjectResolver>();
            var eventStore = new Mock<IEventStore>();

            resolver.Setup(x => x.New<Location>()).Returns(new Location()).Verifiable();

            var domainModel = new RuntimeModel(new BoundedContextModel().WithCommandHandler<MoveIn, Location, MovedIn>().WithDomainObjectResolver(resolver.Object), eventStore.Object);

            domainModel.Aggregates.RetrieveById<Location>(string.Empty);

            resolver.VerifyAll();
        }
    }
}