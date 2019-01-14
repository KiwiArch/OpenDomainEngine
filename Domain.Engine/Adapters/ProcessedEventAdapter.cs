namespace Ode.Domain.Engine.Adapters
{
    using System;

    internal class ProcessedEventAdapter<TEvent> : EventAdapter<TEvent>
    {
        public ProcessedEventAdapter(string processId, int processStep, Type processType, IEvent<TEvent> orignalEvent)
            :base(processId, processStep, processType.FullName, orignalEvent.CommandId, orignalEvent.CorrelationId, orignalEvent.EventBody)
        {
        }
    }
}
