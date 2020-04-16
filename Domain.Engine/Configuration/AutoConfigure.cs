namespace Ode.Domain.Engine.Model.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class AutoConfigure
    {
        public static void FromAssembly(IBoundedContextModel contextMap, Assembly assembly, string contextNamespace)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var publicTypes = contextNamespace == null ? assembly.GetExportedTypes() : assembly.GetExportedTypes().Where(t => t.Namespace.StartsWith(contextNamespace));

            AutoConfigure.MapAggregates(contextMap, publicTypes);

            var allPublicTypesExceptAggregates = publicTypes.Where(t => !contextMap.IsAggregateType(t)).Distinct();

            AutoConfigure.MapEventHandlers(contextMap, allPublicTypesExceptAggregates);
        }

        /// <summary>
        /// Aggregates are identified by having method(s) that:
        /// are named "When"
        /// have a single parameter 
        /// with a type from the same or child namespace
        /// and a matching method named Then whos parameter is the same as the return type of the When method
        /// </summary>
        public static void MapAggregates(IBoundedContextModel contextMap, IEnumerable<Type> publicTypes)
        {
            foreach (var publicType in publicTypes)
            {
                MapAggregate(contextMap, publicType);
            }
        }

        public static void MapAggregate(IBoundedContextModel contextMap, Type aggregateType)
        {
            var methods = aggregateType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();

                if (parameters.Count() == 1
                    && parameters[0].ParameterType.Namespace != null
                    && parameters[0].ParameterType.Namespace.StartsWith(aggregateType.Namespace)
                    && method.Name == "When")
                {
                    if (methods.Any(m => m.Name == "Then" && m.GetParameters().FirstOrDefault(p => p.ParameterType == method.ReturnType) != null))
                    {
                        CallGenericMethod(nameof(ApplyCommandHandlerToContextMap), contextMap, aggregateType, method.ReturnType, parameters);
                    }
                    else
                    {
                        var eventTypes = methods.Where(m => m.Name == "Then" && m.GetParameters().Count() == 1).Select(m => m.GetParameters().Single().ParameterType);

                        var returnTypeProperties = method.ReturnType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        if (returnTypeProperties.All(p => eventTypes.Contains(p.PropertyType)))
                        {
                            returnTypeProperties.Select(p => p.PropertyType).ToList().ForEach(t => CallGenericMethod(nameof(ApplyCommandHandlerToContextMap), contextMap, aggregateType, t, parameters));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Eventhandlers are identified by not being aggregates and having method(s):
        /// named When
        /// with a single parameter that is a known event type and returns a command
        /// </summary>
        /// <param name="publicTypes"></param>
        public static void MapEventHandlers(IBoundedContextModel contextMap, IEnumerable<Type> publicTypes)
        {
            foreach (var publicType in publicTypes)
            {
                MapEventHandler(contextMap, publicType);
            }
        }

        public static void MapEventHandler(IBoundedContextModel contextMap, Type eventHandlerType)
        {
            var methods = eventHandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var method in methods.Where(m => m.Name == "When"))
            {
                var parameters = method.GetParameters();

                if (parameters.Count() == 1 && contextMap.IsEventType(parameters[0].ParameterType))
                {
                    if (method.ReturnType != null && contextMap.IsCommandType(method.ReturnType))
                    {
                        CallGenericMethod(nameof(ApplyProcessHandlerToContextMap), contextMap, eventHandlerType, method.ReturnType, parameters);
                    }
                    else
                    {
                        CallGenericMethod(nameof(ApplyEventHandlerToContextMap), contextMap, eventHandlerType, typeof(void), parameters);
                    }
                }
            }
        }

        private static void CallGenericMethod(string methodName, IBoundedContextModel contextMap, Type publicType, Type returnType, ParameterInfo[] parameters)
        {
            MethodInfo applyToContextMapInfo = typeof(AutoConfigure).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);

            MethodInfo genericMethod;

            if (returnType == typeof(void))
            {
                genericMethod = applyToContextMapInfo.MakeGenericMethod(parameters[0].ParameterType, publicType);
            }
            else
            {
                genericMethod = applyToContextMapInfo.MakeGenericMethod(parameters[0].ParameterType, publicType, returnType);
            }

            genericMethod.Invoke(null, new object[] { contextMap });
        }

        private static void ApplyCommandHandlerToContextMap<TCommand, TAggregate, TEvent>(IBoundedContextModel contextMap)
        {
            contextMap.WithCommandHandler<TCommand, TAggregate, TEvent>();
        }

        private static void ApplyEventHandlerToContextMap<TEvent, TEventHandler>(IBoundedContextModel contextMap) where TEventHandler : class
        {
            contextMap.WithEventHandler<TEvent, TEventHandler>((e) => e.Id);
        }

        private static void ApplyProcessHandlerToContextMap<TEvent, TEventHandler, TCommand>(IBoundedContextModel contextMap) where TEventHandler : class
        {
            contextMap.WithEventHandler<TEvent, TEventHandler, TCommand>((e) => e.Id, c => c.GetType().GetProperties().First().GetValue(c).ToString());
        }
    }
}
