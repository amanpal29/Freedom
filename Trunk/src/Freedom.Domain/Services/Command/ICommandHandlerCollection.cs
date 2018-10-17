using System;
using System.Collections.Generic;
using System.Reflection;
using Freedom.Domain.CommandModel;

namespace Freedom.Domain.Services.Command
{
    public interface ICommandHandlerCollection : IList<ICommandHandler>
    {
        ICommandHandler FindHandlerForCommand(CommandBase command);

        void ScanAssemblyForHandlers(Assembly assembly, Func<Type, bool> predicate = null);
    }
}