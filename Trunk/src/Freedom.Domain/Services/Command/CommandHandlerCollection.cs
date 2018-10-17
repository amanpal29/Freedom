using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Freedom.Domain.CommandModel;
using log4net;

namespace Freedom.Domain.Services.Command
{
    public class CommandHandlerCollection : List<ICommandHandler>, ICommandHandlerCollection
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ICommandHandler FindHandlerForCommand(CommandBase command)
        {
            return this.FirstOrDefault(h => h.CanHandle(command));
        }

        public void ScanAssemblyForHandlers(Assembly assembly, Func<Type, bool> predicate = null)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            IEnumerable<Type> implementations = assembly.GetTypes()
                .Where(typeof (ICommandHandler).IsAssignableFrom)
                .Where(ct => ct.IsClass && !ct.IsAbstract && !ct.IsGenericTypeDefinition);

            if (predicate != null)
            {
                implementations = implementations.Where(predicate);
            }

            foreach (Type concreteType in implementations.Where(t => t.GetConstructor(Type.EmptyTypes) != null))
            {
                try
                {
                    Add((ICommandHandler) Activator.CreateInstance(concreteType));
                }
                catch (TargetInvocationException e)
                {
                    Log.Warn(
                        $"Unable to create instance of command handler {concreteType.FullName}. Command handler was not registered.",
                        e);
                }
            }
        }
    }
}
