namespace Ode.Domain.Engine
{
    using System;

    public interface IEventSubscriber
    {
        void AddSubscriptionHandler<T>(Action<IEventHeader, T> handler);
        void StartSubscription();
    }
}
