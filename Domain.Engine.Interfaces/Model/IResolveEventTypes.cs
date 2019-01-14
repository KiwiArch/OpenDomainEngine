namespace Ode.Domain.Engine.Model
{
    using System;

    public interface IResolveEventTypes
    {
        Type ResolveEventType(string eventTypeFullName);

        string ResolveEventTypeFullName(Type eventType);
    }
}
