using System;

namespace Ode.Domain.Engine.Model
{
    public interface IResolveCommandTypes
    {
        Type ResolveCommandType(string commandTypeFullName);

        string ResolveCommandTypeFullName(Type commandType);
    }
}
