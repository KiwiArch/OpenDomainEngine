namespace Ode.Domain.Engine
{
    using System;
    using System.Collections.Generic;
    using Ode.Domain.Engine.Model;

    internal class CommandHandler : ICommandHandler
    {
        private readonly IBoundedContextModel contextModel;
        private readonly IRuntimeModel runtimeModel;

        public CommandHandler(IBoundedContextModel contextModel, IRuntimeModel runtimeModel)
        {
            this.contextModel = contextModel;
            this.runtimeModel = runtimeModel;
        }

        public IEnumerable<IEvent> HandleCommand(ICommand command, string aggregateId, Type aggregateType)
        {
            var defaultInstance = this.contextModel.DomainObjectResolver.New(aggregateType);

            var aggregateAdapter = this.runtimeModel.Aggregates.RetrieveById(aggregateId, defaultInstance);

            var results = aggregateAdapter.ProcessCommand(command);

            this.runtimeModel.Store();

            return results;
        }
    }
}
