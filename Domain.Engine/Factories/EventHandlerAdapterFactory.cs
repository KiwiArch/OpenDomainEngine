namespace Ode.Domain.Engine.Factories
{
    using Adapters;
    using Model;

    internal class EventHandlerAdapterFactory : IEventHandlerAdapterFactory
    {
        private readonly static EventHandlerAdapterFactory defaultInstance = new EventHandlerAdapterFactory();

        public static EventHandlerAdapterFactory Default { get { return defaultInstance; } }

        public IEventHandlerAdapter<TProcess> CreateEventHandler<TProcess>(string id, TProcess process, IBoundedContextModel contextMap)
        {
            return new EventHandlerAdapter<TProcess>(process, contextMap).WithId(id);
        }
    }
}
