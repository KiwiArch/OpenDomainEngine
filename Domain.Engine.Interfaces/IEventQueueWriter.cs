namespace Ode.Domain.Engine
{
    public interface IEventQueueWriter
    {
        void Enqueue(IEvent domainEvent);
    }
}
