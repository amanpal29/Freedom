CREATE TABLE [ApplicationSetting] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[CreatedDateTime] datetime2 NOT NULL,
	[ModifiedDateTime] datetime2 NOT NULL,
	[CreatedById] uniqueidentifier NOT NULL,
	[ModifiedById] uniqueidentifier NOT NULL,
	[Key] nvarchar(4000) NOT NULL,
	[Value] nvarchar(4000) NULL,
	CONSTRAINT [PK_ApplicationSetting] PRIMARY KEY NONCLUSTERED ( [Id] )
);

GO

CREATE TABLE [Notification] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[Class] int NOT NULL,
	[Priority] int NOT NULL,
	[Title] nvarchar(4000) NULL,
	[Description] nvarchar(4000) NULL,
	[Payload] nvarchar(max) NULL,
	[CreatedDateTime] datetime2 NOT NULL,
	[ReceivedDateTime] datetimeoffset NULL,
	[ReadDateTime] datetimeoffset NULL,
	[CreatedById] uniqueidentifier NULL,
	[RecipientId] uniqueidentifier NOT NULL,
	CONSTRAINT [PK_Notification] PRIMARY KEY NONCLUSTERED ( [Id] )
);

GO

CREATE TABLE [Permission] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[Description] nvarchar(4000) NULL,
	[RoleId] uniqueidentifier NOT NULL,
	CONSTRAINT [PK_Permission] PRIMARY KEY NONCLUSTERED ( [Id] )
);

GO

CREATE TABLE [Role] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[CreatedDateTime] datetime2 NOT NULL,
	[ModifiedDateTime] datetime2 NOT NULL,
	[CreatedById] uniqueidentifier NOT NULL,
	[ModifiedById] uniqueidentifier NOT NULL,
	[Name] nvarchar(4000) NOT NULL,
	CONSTRAINT [PK_Role] PRIMARY KEY NONCLUSTERED ( [Id] )
);

GO

CREATE TABLE [User] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[CreatedDateTime] datetime2 NOT NULL,
	[ModifiedDateTime] datetime2 NOT NULL,
	[CreatedById] uniqueidentifier NOT NULL,
	[ModifiedById] uniqueidentifier NOT NULL,
	[Name] nvarchar(4000) NULL,
	[Username] nvarchar(4000) NULL,
	[Password] nvarchar(4000) NULL,
	[SymmetricKey] nvarchar(4000) NULL,
	[IsActive] bit NOT NULL default 1,
	[Domain] nvarchar(4000) NULL,
	[FirstName] nvarchar(4000) NULL,
	[MiddleName] nvarchar(4000) NULL,
	[LastName] nvarchar(4000) NULL,
	[WorkEmailAddress] nvarchar(4000) NULL,
	[HomeEmailAddress] nvarchar(4000) NULL,
	[HomeAddress_Street] nvarchar(4000) NULL,
	[HomeAddress_PostalCode] nvarchar(4000) NULL,
	[HomeAddress_City] nvarchar(4000) NULL,
	[HomeAddress_Province] nvarchar(4000) NULL,
	[HomeAddress_Country] nvarchar(4000) NULL,
	[ForcePasswordChange] bit NOT NULL default 1,
	CONSTRAINT [PK_User] PRIMARY KEY NONCLUSTERED ( [Id] )
);

GO

CREATE TABLE [UserRole] (
	[UserId] uniqueidentifier NOT NULL,
	[RoleId] uniqueidentifier NOT NULL,
	CONSTRAINT [PK_UserRole] PRIMARY KEY NONCLUSTERED ( [UserId], [RoleId] )
);

GO

CREATE INDEX [IDX_CreatedById]
ON [ApplicationSetting] ( [CreatedById] );

ALTER TABLE [ApplicationSetting]
ADD CONSTRAINT [FK_ApplicationSetting_User_CreatedBy]
FOREIGN KEY ( [CreatedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_ModifiedById]
ON [ApplicationSetting] ( [ModifiedById] );

ALTER TABLE [ApplicationSetting]
ADD CONSTRAINT [FK_ApplicationSetting_User_ModifiedBy]
FOREIGN KEY ( [ModifiedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_RoleId]
ON [Permission] ( [RoleId] );

ALTER TABLE [Permission]
ADD CONSTRAINT [FK_Permission_Role]
FOREIGN KEY ( [RoleId] ) REFERENCES [Role] ( [Id] ) ON DELETE CASCADE;

GO

CREATE INDEX [IDX_CreatedById]
ON [Role] ( [CreatedById] );

ALTER TABLE [Role]
ADD CONSTRAINT [FK_Role_User_CreatedBy]
FOREIGN KEY ( [CreatedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_ModifiedById]
ON [Role] ( [ModifiedById] );

ALTER TABLE [Role]
ADD CONSTRAINT [FK_Role_User_ModifiedBy]
FOREIGN KEY ( [ModifiedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_RoleId]
ON [UserRole] ( [RoleId] );

ALTER TABLE [UserRole]
ADD CONSTRAINT [FK_UserRole_Role]
FOREIGN KEY ( [RoleId] ) REFERENCES [Role] ( [Id] );

GO

CREATE INDEX [IDX_UserId]
ON [UserRole] ( [UserId] );

ALTER TABLE [UserRole]
ADD CONSTRAINT [FK_UserRole_User]
FOREIGN KEY ( [UserId] ) REFERENCES [User] ( [Id] ) ON DELETE CASCADE;

GO

CREATE INDEX [IDX_CreatedById]
ON [User] ( [CreatedById] );

CREATE INDEX [IDX_ModifiedById]
ON [User] ( [ModifiedById] );

CREATE UNIQUE INDEX UNQ_Key
ON [ApplicationSetting] ( [Key] );

GO

CREATE INDEX IDX_Class
ON [Notification] ( Class );

GO

CREATE INDEX IDX_Priority
ON [Notification] ( Priority );

GO

CREATE INDEX IDX_Id
ON [User] ( [Id], [Name], [Username] );

GO

CREATE UNIQUE INDEX UNQ_DomainUsername
ON [User] ( [Domain], [Username] );

GO

