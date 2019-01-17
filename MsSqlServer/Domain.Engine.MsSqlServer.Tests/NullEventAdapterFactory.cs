using System;
using Ode.Domain.Engine;
using Ode.Domain.Engine.Factories;

namespace Ode.Domain.Engine.MsSqlServerTests
{
    public class NullEventAdapterFactory : IEventFactory
    {
        public IEvent<T> CreateEvent<T>(string aggregateId, int aggregateVersion, string aggregateType, string commandId, string correlationId, T domainEvent)
        {
            return null;
        }

        public IEvent<TEvent> CreateEvent<TAggregate, TEvent>(string aggregateId, int aggregateVersion, string commandId, string correlationId, TEvent domainEvent)
        {
            return null;
        }
    }
}
