namespace Ode.Domain.Engine.Model
{
    using System;

    public interface IDomainObjectResolver
    {
        TDomainObject New<TDomainObject>();

        dynamic New(Type type); 
    }
}
