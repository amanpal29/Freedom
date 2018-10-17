using System;
using System.Reflection;

namespace Freedom.Domain.Infrastructure.ExceptionHandling
{
    [Flags]
    public enum ExceptionContextFlags
    {
        None = 0x0000,
        CanRetry = 0x0001,
        CanCancel = 0x0002,
        LastChance = 0x0004
    }

    public sealed class ExceptionContext
    {
        #region Constructor

        public ExceptionContext(MethodBase methodContext, ExceptionContextFlags contextFlags)
        {
            MethodContext = methodContext;
            ContextFlags = contextFlags;
        }

        #endregion

        #region Properties

        public MethodBase MethodContext { get; }

        public ExceptionContextFlags ContextFlags { get; }

        public bool CanRetry => ContextFlags.HasFlag(ExceptionContextFlags.CanRetry);

        public bool CanCancel => ContextFlags.HasFlag(ExceptionContextFlags.CanCancel);

        public bool IsLastChance => ContextFlags.HasFlag(ExceptionContextFlags.LastChance);

        #endregion
    }
}