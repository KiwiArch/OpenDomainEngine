using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Ode.Domain.Engine.SampleModel;
using Ode.Domain.Engine.SampleModel.Adjustments;
using Ode.Domain.Engine.SampleModel.InventoryItems;
using Ode.Domain.Engine.SampleModel.Locations;
using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;
using Ode.Domain.Engine.SampleModel.Movements;
using Ode.Domain.Engine.SampleModel.Transactions.Events;
using Ode.Domain.Engine.Tests.Model.Experimental;
using Ode.Domain.Engine.Factories;
using Ode.Domain.Engine.InMemory.Repositories;
using Ode.Domain.Engine.Model;
using Ode.Domain.Engine.Model.Configuration;
using Ode.Domain.Engine.Model.Fakes;
using Ode.Domain.Engine.Repositories.Fakes;
using Unity;

namespace Ode.Domain.Engine.Tests
{
    [TestClass]
    public class DomainExecutionEngineTests
    {
        [TestMethod]
        public void ProcessCommandUsingCustomHandlerTest()
        {
            var commandId = "command1";
            var correlationId = "command1";
            var aggregateId = "location1";

            var contextModel = new BoundedContextModel()
                .WithAssemblyContaining<Location>()
                .WithCommandHandler<MoveIn, Location, MovedIn>();

            var eventStore = new InMemoryEventStore();

            var item = new StockItem("item", "123");

            var command = CommandFactory.Default.CreateCommand<MoveIn>(commandId, correlationId, aggregateId, new MoveIn($"movement_{Guid.NewGuid()}", "locationA", item, "locationB"));

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            engine.Process(command);
        }

        [TestMethod]
        public void ProcessCommandUsingDefaultHandlerTest()
        {
            var commandId = "command1";
            var correlationId = "command1";
            var aggregateId = "location1";

            var eventStore = new StubIEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>();

            eventStore.RetrieveByIdString = (id) => new Collection<IEvent>();

            var item = new StockItem("item", "123");

            var command = CommandFactory.Default.CreateCommand<MoveIn>(commandId, correlationId, aggregateId, new MoveIn($"movement_{Guid.NewGuid()}", "locationA", item, "locationB"));

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            engine.Process(command);
        }

        [TestMethod]
        public void ProcessTwoSequentialCommandsTest()
        {
            var eventStore = new InMemoryEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>();

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            var item = new StockItem("item", "123");

            var results1 = engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c1", "c1", "ar1", new CreateLocation("locationA")));
            var results2 = engine.Process(CommandFactory.Default.CreateCommand<AdjustIn>("c2", "c1", "ar1", new AdjustIn($"adjustment_{Guid.NewGuid()}", "locationA", item)));

            Assert.IsNotNull(results1.FirstOrDefault(e => e.EventBody is LocationCreated));
            Assert.IsNotNull(results2.FirstOrDefault(e => e.EventBody is AdjustedIn));
        }

        [TestMethod]
        public void ProcessTwoSequentialCommandsUsingDefaultHandlersTest()
        {
            var eventStore = new StubIEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>();

            eventStore.RetrieveByIdString = (id) => new Collection<IEvent>();

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            var item = new StockItem("item", "123");

            var results1 = engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c1", "c1", "ar1", new CreateLocation("locationA")));
            var results2 = engine.Process(CommandFactory.Default.CreateCommand<AdjustIn>("c2", "c1", "ar1", new AdjustIn($"adjustment_{Guid.NewGuid()}", "locationA", item)));

            Assert.IsNotNull(results1.FirstOrDefault(e => e.EventBody is LocationCreated));
            Assert.IsNotNull(results2.FirstOrDefault(e => e.EventBody is AdjustedIn));
        }

        [TestMethod]
        public void SimpleProcessUpdatesTwoAggregates()
        {
            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>()
                .WithEventHandler<MovedOut, Movement, MoveIn>((e) => e.EventBody.Movement.ToString(), c => c.Location)
                .WithEventHandler<MovedIn, Movement>((e) => e.EventBody.Movement.ToString());

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            var item = new StockItem("item", "123");

            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c1", "c1", "ar1", new CreateLocation("ar1")));
            engine.Process(CommandFactory.Default.CreateCommand<AdjustIn>("c2", "c1", "ar1", new AdjustIn($"adjustment_{Guid.NewGuid()}", "ar1", item)));
            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c3", "c3", "ar2", new CreateLocation("ar2")));

            var results = engine.Process(CommandFactory.Default.CreateCommand<MoveOut>("c3", "c3", "ar1", new MoveOut($"movement_{Guid.NewGuid()}", "ar1", item, "ar2")));

            var movedOutEvent = results.FirstOrDefault(e => e.EventBody is MovedOut);
            var movedInEvent = results.FirstOrDefault(e => e.EventBody is MovedIn);

            Assert.IsNotNull(movedOutEvent);
            Assert.IsNotNull(movedInEvent);
        }

        [TestMethod]
        public void SimpleProcessUpdatesTwoAggregatesUsingDefaultHandlersTest()
        {
            var eventStore = new InMemoryEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>();
            var aggregateModel = new RuntimeModel(contextModel, eventStore);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            var item = new StockItem("item", "123");

            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c1", "c1", "ar1", new CreateLocation("ar1")));
            engine.Process(CommandFactory.Default.CreateCommand<AdjustIn>("c2", "c1", "ar1", new AdjustIn($"adjustment_{Guid.NewGuid()}", "ar1", item)));
            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c3", "c3", "ar2", new CreateLocation("ar2")));

            var results = engine.Process(CommandFactory.Default.CreateCommand<MoveOut>("c3", "c3", "ar1", new MoveOut($"movement_{Guid.NewGuid()}", "ar1", item, "ar2")));

            var movedOutEvent = results.FirstOrDefault(e => e.EventBody is MovedOut);
            var movedInEvent = results.FirstOrDefault(e => e.EventBody is MovedIn);

            Assert.IsNotNull(movedOutEvent);
            Assert.IsNotNull(movedInEvent);
        }

        private static List<StockItem> items = new List<StockItem>();

        public class TestEventHandler
        {
            public void When(MovedIn movedIn)
            {
                items.Add(movedIn.Item);
            }
        }

        [TestMethod]
        public void SimpleProcessUpdatesTwoAggregatesAndFinalEventHandlerUsingDefaultHandlersTest()
        {
            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel()
                .WithAssemblyContaining<Location>()
                .WithEventHandler<TestEventHandler>();

            var aggregateModel = new RuntimeModel(contextModel, eventStore);
            var item = new StockItem("item", "123");

            items.Clear();

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c1", "c1", "ar1", new CreateLocation("ar1")));
            engine.Process(CommandFactory.Default.CreateCommand<AdjustIn>("c2", "c1", "ar1", new AdjustIn($"adjustment_{Guid.NewGuid()}", "ar1", item)));
            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c3", "c3", "ar2", new CreateLocation("ar2")));

            var results = engine.Process(CommandFactory.Default.CreateCommand<MoveOut>("c3", "c3", "ar1", new MoveOut($"movement_{Guid.NewGuid()}", "ar1", item, "ar2")));

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(item, items[0]);
        }

        [TestMethod]
        public void IoCUsingCustomMethodTest()
        {
            var newLocationWasCalled = false;
            var newInventoryItemWasCalled = false;

            var domainObjectResolver = new StubIDomainObjectResolver();

            domainObjectResolver.NewOf1<Location>(() => { newLocationWasCalled = true; return new Location(); });
            domainObjectResolver.NewOf1<InventoryItem>(() => { newInventoryItemWasCalled = true; return new InventoryItem(); });
            domainObjectResolver.NewType = (t) => Activator.CreateInstance(t);

            var eventStore = new StubIEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>().WithDomainObjectResolver(domainObjectResolver);

            eventStore.RetrieveByIdString = (id) => new Collection<IEvent>();

            var command1 = CommandFactory.Default.CreateCommand<CreateLocation>(id: "C1", aggregateId: "Location 1", command: new CreateLocation("Location 1"));
            var command2 = CommandFactory.Default.CreateCommand<CreateInventoryItem>(id: "C2", aggregateId: "Item A", command: new CreateInventoryItem("Item A"));

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            engine.Process(command1);
            engine.Process(command2);


            Assert.IsTrue(newLocationWasCalled && newInventoryItemWasCalled);
        }

        private class UnityDomainObjectFactory : IDomainObjectResolver
        {
            private readonly IUnityContainer container;

            public UnityDomainObjectFactory(UnityContainer container)
            {
                this.container = container;
            }

            public object New(Type type)
            {
                return this.container.Resolve(type);
            }

            public TDomainObject New<TDomainObject>()
            {
                return this.container.Resolve<TDomainObject>();
            }
        }

        [TestMethod]
        public void IocUsingUnityTest()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<Location>();
            unityContainer.RegisterType<InventoryItem>();

            var domainObjectFactory = new UnityDomainObjectFactory(unityContainer);

            var eventStore = new StubIEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>().WithDomainObjectResolver(domainObjectFactory);

            eventStore.RetrieveByIdString = (id) => new Collection<IEvent>();

            var command1 = CommandFactory.Default.CreateCommand<CreateLocation>(id: "C1", aggregateId: "Location 1", command: new CreateLocation("Location 1"));
            var command2 = CommandFactory.Default.CreateCommand<CreateInventoryItem>(id: "C2", aggregateId: "Item A", command: new CreateInventoryItem("Item A"));

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            engine.Process(command1);
            engine.Process(command2);
        }

        private class NinjectDomainObjectFactory : IDomainObjectResolver
        {
            private readonly StandardKernel container;

            public NinjectDomainObjectFactory(StandardKernel container)
            {
                this.container = container;
            }

            public object New(Type type)
            {
                return this.container.Get(type);
            }

            public TDomainObject New<TDomainObject>()
            {
                return this.container.Get<TDomainObject>();
            }
        }

        [TestMethod]
        public void IoCUsingNijectTest()
        {
            var ninjectKernel = new StandardKernel();
            ninjectKernel.Bind<Location>().To<Location>();
            ninjectKernel.Bind<InventoryItem>().To<InventoryItem>();

            var domainObjectFactory = new NinjectDomainObjectFactory(ninjectKernel);

            var eventStore = new StubIEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>().WithDomainObjectResolver(domainObjectFactory);
            var domainModel = new RuntimeModel(contextModel, eventStore);

            eventStore.RetrieveByIdString = (id) => new Collection<IEvent>();

            var command1 = CommandFactory.Default.CreateCommand<CreateLocation>(id: "C1", aggregateId: "Location 1", command: new CreateLocation("Location 1"));
            var command2 = CommandFactory.Default.CreateCommand<CreateInventoryItem>(id: "C2", aggregateId: "Item A", command: new CreateInventoryItem("Item A"));

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            engine.Process(command1);
            engine.Process(command2);
        }

        [TestMethod]
        public void FormaterServicesIsTypeSafe()
        {
            var e = FormatterServices.GetUninitializedObject(typeof(LocationCreated)) as dynamic;

            Assert.IsInstanceOfType(e, typeof(LocationCreated));
        }

        [TestMethod]
        public void CommandIsIdempotent()
        {
            string aggregateId = "ar1";
            string commandId = "c1";
            string adjustmentId = "adjust1";
            var item = new StockItem("item", "123");


            var eventStore = new InMemoryEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>();

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>("c0", "c0", aggregateId, new CreateLocation(aggregateId))).Single();

            var command = CommandFactory.Default.CreateCommand<AdjustIn>(commandId, commandId, aggregateId, new AdjustIn(adjustmentId, aggregateId, item));

            var firstResults = engine.Process(command);
            var secondResults = engine.Process(command);

            Assert.IsTrue(firstResults.Any(e => e.EventBody is AdjustedIn));
            Assert.IsFalse(secondResults.Any(e => e.EventBody is AdjustedIn));
        }

        [TestMethod]
        public void AggregateEventTest()
        {
            string aggregateId = "ar1";
            string commandId = "c1";

            var eventStore = new InMemoryEventStore();
            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>();

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, DomainOptions.Defaults);

            var result = engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>(commandId, commandId, aggregateId, new CreateLocation(aggregateId))).Single();

            Assert.IsInstanceOfType(result.EventBody, typeof(LocationCreated));
            Assert.IsTrue(result.AggregateId == aggregateId);
            Assert.IsTrue(result.AggregateType == typeof(Location).FullName);
            Assert.IsTrue(result.AggregateVersion == 1);
            Assert.IsTrue(result.CommandId == commandId);
            Assert.IsTrue(result.CorrelationId == commandId);
            Assert.IsTrue(result.EventBodyType == typeof(LocationCreated).FullName);
            Assert.IsTrue(result.Id == $"{aggregateId}\\{result.AggregateVersion}");
        }

        [TestMethod]
        public void AggregateAndStatelessProcessEventsTest()
        {
            string firstAggregateId = "ar1";
            string secondAggregateId = "ar2";
            string firstCommandId = "c1";
            string secondCommandId = "c2";
            string thirdCommandId = "c3";
            string fourthCommandId = "c4";
            var item = new StockItem("item", "123");
            string movementNumber = $"movement_{Guid.NewGuid()}";

            var eventStore = new InMemoryEventStore();
            var contextModel = new BoundedContextModel()
                .WithAggregateRoot<Location>()
                .WithEventHandler<MovedOut, Movement, MoveIn>(e => e.EventBody.Movement, c => c.Location);

            var options = new DomainOptions(DomainOption.CacheRuntimeModel);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, options);

            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>(firstCommandId, firstCommandId, firstAggregateId, new CreateLocation(firstAggregateId)));
            engine.Process(CommandFactory.Default.CreateCommand<AdjustIn>(secondCommandId, secondCommandId, firstAggregateId, new AdjustIn($"adjustment_{Guid.NewGuid()}", firstAggregateId, item)));
            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>(thirdCommandId, thirdCommandId, secondAggregateId, new CreateLocation(secondAggregateId)));

            var results = engine.Process(CommandFactory.Default.CreateCommand<MoveOut>(fourthCommandId, fourthCommandId, firstAggregateId, new MoveOut(movementNumber, firstAggregateId, item, secondAggregateId))).ToList();

            Assert.IsTrue(results.Count == 2);

            Assert.IsInstanceOfType(results[0].EventBody, typeof(MovedOut));
            Assert.IsTrue(results[0].AggregateId == firstAggregateId);
            Assert.IsTrue(results[0].AggregateType == typeof(Location).FullName);
            Assert.IsTrue(results[0].AggregateVersion == 3);
            Assert.IsTrue(results[0].CommandId == fourthCommandId);
            Assert.IsTrue(results[0].CorrelationId == fourthCommandId);
            Assert.IsTrue(results[0].EventBodyType == typeof(MovedOut).FullName);
            Assert.IsTrue(results[0].Id == $"{firstAggregateId}\\{results[0].AggregateVersion}");

            Assert.IsInstanceOfType(results[1].EventBody, typeof(MovedIn));
            Assert.IsTrue(results[1].AggregateId == secondAggregateId);
            Assert.IsTrue(results[1].AggregateType == typeof(Location).FullName);
            Assert.IsTrue(results[1].AggregateVersion == 2);
            Assert.IsTrue(results[1].CommandId == $"{typeof(Movement).Name}\\{results[0].EventBody.Movement}\\{1}");
            Assert.IsTrue(results[1].CorrelationId == fourthCommandId);
            Assert.IsTrue(results[1].EventBodyType == typeof(MovedIn).FullName);
            Assert.IsTrue(results[1].Id == $"{secondAggregateId}\\{results[1].AggregateVersion}");
        }

        [TestMethod]
        public void AggregateAndStatefulProcessEventsTest()
        {
            string firstAggregateId = "ar1";
            string secondAggregateId = "ar2";
            string firstCommandId = "c1";
            string secondCommandId = "c2";
            string thirdCommandId = "c3";
            string fourthCommandId = "c4";
            var item = new StockItem("item", "123");
            string movementNumber = $"movement_{Guid.NewGuid()}";

            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel() //.WithAssemblyContaining<Location>()
                .WithAggregateRoot<Location>()
                .WithEventHandler<MovedOut, Movement, MoveIn>(e => e.EventBody.Movement.ToString(), c => c.Location)
                .WithEventHandler<MovedIn, Movement>(e => e.EventBody.Movement.ToString());

            var options = new DomainOptions(DomainOption.CacheRuntimeModel);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, options);

            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>(firstCommandId, firstCommandId, firstAggregateId, new CreateLocation(firstAggregateId)));
            engine.Process(CommandFactory.Default.CreateCommand<AdjustIn>(secondCommandId, secondCommandId, firstAggregateId, new AdjustIn($"adjustment_{Guid.NewGuid()}", firstAggregateId, item)));
            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>(thirdCommandId, thirdCommandId, secondAggregateId, new CreateLocation(secondAggregateId)));

            var results = engine.Process(CommandFactory.Default.CreateCommand<MoveOut>(fourthCommandId, fourthCommandId, firstAggregateId, new MoveOut(movementNumber, firstAggregateId, item, secondAggregateId))).ToList();

            Assert.IsTrue(results.Count == 2);

            Assert.IsInstanceOfType(results[0].EventBody, typeof(MovedOut));
            Assert.IsTrue(results[0].AggregateId == firstAggregateId);
            Assert.IsTrue(results[0].AggregateType == typeof(Location).FullName);
            Assert.IsTrue(results[0].AggregateVersion == 3);
            Assert.IsTrue(results[0].CommandId == fourthCommandId);
            Assert.IsTrue(results[0].CorrelationId == fourthCommandId);
            Assert.IsTrue(results[0].EventBodyType == typeof(MovedOut).FullName);
            Assert.IsTrue(results[0].Id == $"{firstAggregateId}\\{results[0].AggregateVersion}");

            Assert.IsInstanceOfType(results[1].EventBody, typeof(MovedIn));
            Assert.IsTrue(results[1].AggregateId == secondAggregateId);
            Assert.IsTrue(results[1].AggregateType == typeof(Location).FullName);
            Assert.IsTrue(results[1].AggregateVersion == 2);
            Assert.IsTrue(results[1].CommandId == $"{typeof(Movement).Name}\\{movementNumber}\\{1}");
            Assert.IsTrue(results[1].CorrelationId == fourthCommandId);
            Assert.IsTrue(results[1].EventBodyType == typeof(MovedIn).FullName);
            Assert.IsTrue(results[1].Id == $"{secondAggregateId}\\{results[1].AggregateVersion}");
        }

        [TestMethod]
        public void InheritedEventHandlerTest()
        {
            string firstAggregateId = "ar1";
            string secondAggregateId = "ar2";

            string firstCommandId = "c1";
            string secondCommandId = "c2";


            var item = new StockItem("item", "123");
            string movementNumber = $"movement_{Guid.NewGuid()}";

            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel().WithAssemblyContaining<Location>()
                .WithEventHandler<AdjustedIn, Adjustment>(e => e.EventBody.Adjustment);

            var options = new DomainOptions(DomainOption.CacheRuntimeModel);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, options);

            engine.Process(CommandFactory.Default.CreateCommand<CreateLocation>(firstCommandId, firstCommandId, firstAggregateId, new CreateLocation(firstAggregateId)));

            var results = engine.Process(CommandFactory.Default.CreateCommand<MoveIn>(secondCommandId, secondCommandId, firstAggregateId, new MoveIn(movementNumber, firstAggregateId, item, secondAggregateId))).ToList();

            Assert.IsTrue(results.Any(r => r.EventBodyType == typeof(TransactionRecorded).FullName));
        }


        [TestMethod]
        public void MultipleEventsTest()
        {
            string commandId = "c1";

            string experimentId = Guid.NewGuid().ToString();

            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel().WithAssemblyContaining<Experiment>();

            var options = new DomainOptions(DomainOption.CacheRuntimeModel);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, options);

            var results = engine.Process(CommandFactory.Default.CreateCommand<PerformExperiment>(commandId, commandId, experimentId, new PerformExperiment(true, true)));

            Assert.AreEqual(2, results.Count());
        }

        [TestMethod]
        public void MultipleOptionalEventsTest()
        {
            string commandId = "c1";

            string experimentId = Guid.NewGuid().ToString();

            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel().WithAssemblyContaining<Experiment>();

            var options = new DomainOptions(DomainOption.CacheRuntimeModel);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, options);

            var results = engine.Process(CommandFactory.Default.CreateCommand<PerformExperiment>(commandId, commandId, experimentId, new PerformExperiment(true, false)));

            Assert.IsTrue(results.Count() == 1);
        }

        [TestMethod]
        public void MultipleOptionalEventsAllNullTest()
        {
            string commandId = "c1";

            string experimentId = Guid.NewGuid().ToString();

            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel().WithAssemblyContaining<Experiment>();

            var options = new DomainOptions(DomainOption.CacheRuntimeModel);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, options);

            var results = engine.Process(CommandFactory.Default.CreateCommand<PerformExperiment>(commandId, commandId, experimentId, new PerformExperiment(false, false)));

            Assert.IsTrue(results.Count() == 0);
        }

        [TestMethod]
        public void ExeptionIsThrownTest()
        {
            string commandId = "c1";

            string experimentId = Guid.NewGuid().ToString();

            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel().WithAssemblyContaining<Experiment>();

            var options = new DomainOptions(DomainOption.CacheRuntimeModel);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, options);

            Assert.ThrowsException<ApplicationException>(() => { engine.Process(CommandFactory.Default.CreateCommand<PerformBadExperiment>(commandId, commandId, experimentId, new PerformBadExperiment(true))); });
        }


        [TestMethod]
        public void ExeptionIsWrappedInEventTest()
        {
            string commandId = "c1";

            string experimentId = Guid.NewGuid().ToString();

            var eventStore = new InMemoryEventStore();

            var contextModel = new BoundedContextModel().WithAssemblyContaining<Experiment>().WithExceptionEvent<BadEvent>((e) => new BadEvent(e));

            var options = new DomainOptions(DomainOption.CacheRuntimeModel);

            var engine = DomainFactory.CreateDomainExecutionEngine(contextModel, eventStore, options);

            var result = engine.Process(CommandFactory.Default.CreateCommand<PerformBadExperiment>(commandId, commandId, experimentId, new PerformBadExperiment(true)));

            Assert.IsNotNull(result.FirstOrDefault(e => e.EventBody is BadEvent));
        }
    }
}