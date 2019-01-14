namespace Ode.Domain.Engine
{
    using System;

    public interface IDomainMessage
    {
        string Id { get; }

        string CorrelationId { get; }
    }
}
