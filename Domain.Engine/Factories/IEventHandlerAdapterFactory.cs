namespace Ode.Domain.Engine.Factories
{
    using Adapters;
    using Model;

    internal interface IEventHandlerAdapterFactory
    {
        IEventHandlerAdapter<TProcess> CreateEventHandler<TProcess>(string id, TProcess process, IBoundedContextModel contextMap);
    }
}
