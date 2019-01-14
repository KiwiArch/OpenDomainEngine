namespace Ode.Domain.Engine.Brokers
{
    using System.Collections.Generic;

    public interface ICommandBroker
    {
        IEnumerable<IEvent> BrokerCommand<TCommand>(ICommand<TCommand> command);
    }
}
