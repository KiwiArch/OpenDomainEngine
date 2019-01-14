using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ode.Domain.Engine.Model.Fakes;
using Ode.Domain.Engine.Repositories.Fakes;
using Ode.Domain.Engine.SampleModel.Locations;

namespace Ode.Domain.Engine.Model.Tests
{
    [TestClass]
    public class DomainModelTests
    {
        [TestMethod]
        public void DomainModelTest()
        {
            var domainModel = new RuntimeModel(new BoundedContextModel(), new StubIEventStore());

            Assert.IsInstanceOfType(domainModel, typeof(IRuntimeModel));
        }

        //[TestMethod]
        //public void WithDomainObjectResolverTest()
        //{
        //    var resolverCalled = false;

        //    var resolver = new StubIDomainObjectResolver();
        //    resolver.NewOf1<Location>(() => { resolverCalled = true; return new Location(); });

        //    var domainModel = new RuntimeModel(new BoundedContextModel().WithDomainObjectResolver(resolver), new StubIEventStore());

        //    domainModel.Aggregates.RetrieveById<Location>(string.Empty);

        //    Assert.IsTrue(resolverCalled);
        //}
    }
}