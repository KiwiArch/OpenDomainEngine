namespace Ode.Domain.Engine.Model
{
    using Adapters;

    internal interface IEventHandlerRuntimeModel
    {
        IEventHandlerAdapter<TProcess> Create<TProcess>(TProcess process, string id);

        IEventHandlerAdapter<TProcess> RetrieveById<TProcess>(string id, TProcess defaultInstance = null) where TProcess : class;
    }
}
