namespace Ode.Domain.Engine
{
    using System.Collections.Generic;

    public interface IDomainEngine
    {
        IEnumerable<IEvent> Process(ICommand command);
    }
}
