namespace Ode.Domain.Engine
{
    using System;
    using System.Collections.Generic;

    public interface ICommandHandler
    {
        IEnumerable<IEvent> HandleCommand(ICommand command, string aggregateId, Type aggregateType);
    }
}