namespace Freedom.Domain.Model
{
    public class EntityPaths
    {
        internal EntityPaths()
        {
            Host = null;
        }

        internal EntityPaths(string host)
        {
            Host = string.IsNullOrEmpty(host) ? null : host;
        }

        protected string Host { get; }

        protected string PathPrefix => string.IsNullOrEmpty(Host) ? string.Empty : Host + '.';

        public override string ToString()
        {
            return Host;
        }

        public static implicit operator string(EntityPaths entityNames)
        {
            return entityNames.ToString();
        }
    }

	public class EntityBasePaths : EntityPaths
	{
		public EntityBasePaths()
		{
		}
		
		public EntityBasePaths(string hostName)
			: base(hostName)
		{
		}

		public string Id => PathPrefix + "Id";
	}

	public class AggregateRootPaths : EntityPaths
	{
		public AggregateRootPaths()
		{
		}
		
		public AggregateRootPaths(string hostName)
			: base(hostName)
		{
		}

		public UserPaths CreatedBy
			=> new UserPaths(PathPrefix + "CreatedBy");

		public UserPaths ModifiedBy
			=> new UserPaths(PathPrefix + "ModifiedBy");

		public string Id => PathPrefix + "Id";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ModifiedDateTime => PathPrefix + "ModifiedDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string ModifiedById => PathPrefix + "ModifiedById";
	}

	public class NumberedRootPaths : EntityPaths
	{
		public NumberedRootPaths()
		{
		}
		
		public NumberedRootPaths(string hostName)
			: base(hostName)
		{
		}

		public string Id => PathPrefix + "Id";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ModifiedDateTime => PathPrefix + "ModifiedDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string ModifiedById => PathPrefix + "ModifiedById";

		public string Number => PathPrefix + "Number";
	}

	public class LookupBasePaths : EntityPaths
	{
		public LookupBasePaths()
		{
		}
		
		public LookupBasePaths(string hostName)
			: base(hostName)
		{
		}

		public string Id => PathPrefix + "Id";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ModifiedDateTime => PathPrefix + "ModifiedDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string ModifiedById => PathPrefix + "ModifiedById";

		public string Description => PathPrefix + "Description";

		public string SortOrder => PathPrefix + "SortOrder";
	}

	public class LookupPaths : EntityPaths
	{
		public LookupPaths()
		{
		}
		
		public LookupPaths(string hostName)
			: base(hostName)
		{
		}

		public string Id => PathPrefix + "Id";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ModifiedDateTime => PathPrefix + "ModifiedDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string ModifiedById => PathPrefix + "ModifiedById";

		public string Description => PathPrefix + "Description";

		public string SortOrder => PathPrefix + "SortOrder";

		public string IsActive => PathPrefix + "IsActive";
	}

	public class NotificationBasePaths : EntityPaths
	{
		public NotificationBasePaths()
		{
		}
		
		public NotificationBasePaths(string hostName)
			: base(hostName)
		{
		}

		public string Id => PathPrefix + "Id";

		public string Class => PathPrefix + "Class";

		public string Priority => PathPrefix + "Priority";

		public string Title => PathPrefix + "Title";

		public string Description => PathPrefix + "Description";

		public string Payload => PathPrefix + "Payload";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ReceivedDateTime => PathPrefix + "ReceivedDateTime";

		public string ReadDateTime => PathPrefix + "ReadDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string RecipientId => PathPrefix + "RecipientId";
	}

	public class ApplicationSettingPaths : EntityPaths
	{
		public ApplicationSettingPaths()
		{
		}
		
		public ApplicationSettingPaths(string hostName)
			: base(hostName)
		{
		}

		public string Id => PathPrefix + "Id";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ModifiedDateTime => PathPrefix + "ModifiedDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string ModifiedById => PathPrefix + "ModifiedById";

		public string Key => PathPrefix + "Key";

		public string Value => PathPrefix + "Value";
	}

	public class NotificationPaths : EntityPaths
	{
		public NotificationPaths()
		{
		}
		
		public NotificationPaths(string hostName)
			: base(hostName)
		{
		}

		public string Id => PathPrefix + "Id";

		public string Class => PathPrefix + "Class";

		public string Priority => PathPrefix + "Priority";

		public string Title => PathPrefix + "Title";

		public string Description => PathPrefix + "Description";

		public string Payload => PathPrefix + "Payload";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ReceivedDateTime => PathPrefix + "ReceivedDateTime";

		public string ReadDateTime => PathPrefix + "ReadDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string RecipientId => PathPrefix + "RecipientId";
	}

	public class PermissionPaths : EntityPaths
	{
		public PermissionPaths()
		{
		}
		
		public PermissionPaths(string hostName)
			: base(hostName)
		{
		}

		public RolePaths Role
			=> new RolePaths(PathPrefix + "Role");

		public string Id => PathPrefix + "Id";

		public string Description => PathPrefix + "Description";

		public string RoleId => PathPrefix + "RoleId";
	}

	public class RolePaths : EntityPaths
	{
		public RolePaths()
		{
		}
		
		public RolePaths(string hostName)
			: base(hostName)
		{
		}

		public PermissionPaths Permissions
			=> new PermissionPaths(PathPrefix + "Permissions");

		public string Id => PathPrefix + "Id";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ModifiedDateTime => PathPrefix + "ModifiedDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string ModifiedById => PathPrefix + "ModifiedById";

		public string Name => PathPrefix + "Name";
	}

	public class UserPaths : EntityPaths
	{
		public UserPaths()
		{
		}
		
		public UserPaths(string hostName)
			: base(hostName)
		{
		}

		public RolePaths Roles
			=> new RolePaths(PathPrefix + "Roles");

		public UserRolePaths UserRole
			=> new UserRolePaths(PathPrefix + "UserRole");

		public string Id => PathPrefix + "Id";

		public string CreatedDateTime => PathPrefix + "CreatedDateTime";

		public string ModifiedDateTime => PathPrefix + "ModifiedDateTime";

		public string CreatedById => PathPrefix + "CreatedById";

		public string ModifiedById => PathPrefix + "ModifiedById";

		public string Name => PathPrefix + "Name";

		public string Username => PathPrefix + "Username";

		public string Password => PathPrefix + "Password";

		public string SymmetricKey => PathPrefix + "SymmetricKey";

		public string IsActive => PathPrefix + "IsActive";

		public string Domain => PathPrefix + "Domain";

		public string FirstName => PathPrefix + "FirstName";

		public string MiddleName => PathPrefix + "MiddleName";

		public string LastName => PathPrefix + "LastName";

		public string WorkEmailAddress => PathPrefix + "WorkEmailAddress";

		public string HomeEmailAddress => PathPrefix + "HomeEmailAddress";

		public string ForcePasswordChange => PathPrefix + "ForcePasswordChange";
	}

	public class UserRolePaths : EntityPaths
	{
		public UserRolePaths()
		{
		}
		
		public UserRolePaths(string hostName)
			: base(hostName)
		{
		}

		public UserPaths User
			=> new UserPaths(PathPrefix + "User");

		public RolePaths Role
			=> new RolePaths(PathPrefix + "Role");

		public string UserId => PathPrefix + "UserId";

		public string RoleId => PathPrefix + "RoleId";
	}

	public static class Paths
	{
		public static EntityBasePaths EntityBase => new EntityBasePaths();

		public static AggregateRootPaths AggregateRoot => new AggregateRootPaths();

		public static NumberedRootPaths NumberedRoot => new NumberedRootPaths();

		public static LookupBasePaths LookupBase => new LookupBasePaths();

		public static LookupPaths Lookup => new LookupPaths();

		public static NotificationBasePaths NotificationBase => new NotificationBasePaths();

		public static ApplicationSettingPaths ApplicationSetting => new ApplicationSettingPaths();

		public static NotificationPaths Notification => new NotificationPaths();

		public static PermissionPaths Permission => new PermissionPaths();

		public static RolePaths Role => new RolePaths();

		public static UserPaths User => new UserPaths();

		public static UserRolePaths UserRole => new UserRolePaths();

	}
}
