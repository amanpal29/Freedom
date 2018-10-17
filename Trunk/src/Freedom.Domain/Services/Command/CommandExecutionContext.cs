using System;
using System.Collections.Generic;
using System.Linq;
using Freedom.Domain.Model;
using Freedom.Domain.Services.BackgroundWorkQueue;
using Freedom.Domain.Services.Security;

namespace Freedom.Domain.Services.Command
{
    [Flags]
    public enum CommandExecutionContextOptions
    {
        None = 0x0,
        Offline = 0x1
    }

    public class CommandExecutionContext
    {
        public CommandExecutionContext(Guid currentUserId, DateTime currentDateTime)
        {
            if (currentUserId != User.SuperUserId)
                throw new ArgumentException(
                    "This constructor can only be used when the currentUser is the administrator account.",
                    nameof(currentUserId));

            CurrentUserId = currentUserId;
            CurrentDateTime = currentDateTime;
            IsAdministrator = true;
            Permissions = Enum.GetValues(typeof(SystemPermission)).Cast<SystemPermission>().ToList();
        }

        public CommandExecutionContext(FreedomPrincipal principal, DateTime currentDateTime,
            CommandExecutionContextOptions options)
        {
            CurrentUserId = principal.UserId;
            IsAdministrator = principal.IsAdministrator;
            Permissions = principal.Permissions.Permissions;
            CurrentDateTime = currentDateTime;
            Options = options;
        }

        public Guid CurrentUserId { get; }

        public bool IsAdministrator { get; }

        public ICollection<SystemPermission> Permissions { get; }

        public DateTime CurrentDateTime { get; }

        public CommandExecutionContextOptions Options { get;  }

        public IList<IWorkItem> OnCommitWorkItems { get; } = new List<IWorkItem>();

        public bool IsOffline => Options.HasFlag(CommandExecutionContextOptions.Offline);

        public override string ToString()
        {
            return $"{CurrentUserId} - {CurrentDateTime:O}";
        }
    }
}
