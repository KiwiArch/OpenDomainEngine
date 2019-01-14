namespace Ode.Domain.Engine.Model
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;

    internal class DomainObjectResolver : IDomainObjectResolver
    {
        public object New(Type type)
        {
            return Activator.CreateInstance(type) as dynamic;
        }

        public TDomainObject New<TDomainObject>()
        {
            return Activator.CreateInstance<TDomainObject>();
        }
    }   
}
