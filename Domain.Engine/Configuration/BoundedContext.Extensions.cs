namespace Ode.Domain.Engine.Model.Configuration
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class BoundedContextExtensions
    {
        public static IBoundedContextModel WithAssembly(this IBoundedContextModel contextMap, string assemblyName, string contextNamespace = null)
        {
            var assembly = Assembly.Load(assemblyName);

            if (assembly.IsDynamic)
            {
                throw new ArgumentException("Dynamic assemblies are not supported.");
            }

            contextMap.WithAssembly(assembly, contextNamespace);

            return contextMap;
        }

        public static IBoundedContextModel WithAssemblyContaining<T>(this IBoundedContextModel contextMap, string contextNamespace = null)
        {
            var assembly = typeof(T).Assembly;

            if (assembly.IsDynamic)
            {
                throw new ArgumentException("Dynamic assemblies are not supported.");
            }

            contextMap.WithAssembly(assembly, contextNamespace);

            return contextMap;
        }

        public static IBoundedContextModel WithAllAppDomainAssemblies(this IBoundedContextModel contextMap)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies.Where(a => !a.IsDynamic))
            {
                contextMap.WithAssembly(assembly, null);
            }

            return contextMap;
        }

        public static IBoundedContextModel WithAssembly(this IBoundedContextModel contextMap, Assembly assembly, string contextNamespace)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            AutoConfigure.FromAssembly(contextMap, assembly, contextNamespace);

            return contextMap;
        }

        public static IBoundedContextModel WithAggregateRoot<TAggregateRoot>(this IBoundedContextModel contextMap)
        {
            AutoConfigure.MapAggregate(contextMap, typeof(TAggregateRoot));

            return contextMap;
        }

        public static IBoundedContextModel WithEventHandler<TEventHandler>(this IBoundedContextModel contextMap)
        {
            AutoConfigure.MapEventHandler(contextMap, typeof(TEventHandler));

            return contextMap;
        }
    }
}
