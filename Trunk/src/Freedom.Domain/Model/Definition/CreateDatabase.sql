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

CREATE TABLE [MarketIndex] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[CreatedDateTime] datetime2 NOT NULL,
	[ModifiedDateTime] datetime2 NOT NULL,
	[CreatedById] uniqueidentifier NOT NULL,
	[ModifiedById] uniqueidentifier NOT NULL,
	[Description] nvarchar(4000) NULL,
	[SortOrder] int NOT NULL default 0,
	[IsActive] bit NOT NULL default 1,
	CONSTRAINT [PK_MarketIndex] PRIMARY KEY NONCLUSTERED ( [Id] )
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

CREATE TABLE [Stock] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[CreatedDateTime] datetime2 NOT NULL,
	[ModifiedDateTime] datetime2 NOT NULL,
	[CreatedById] uniqueidentifier NOT NULL,
	[ModifiedById] uniqueidentifier NOT NULL,
	[Symbol] nvarchar(4000) NOT NULL,
	[StockExchangeId] uniqueidentifier NOT NULL,
	CONSTRAINT [PK_Stock] PRIMARY KEY NONCLUSTERED ( [Id] )
);

GO

CREATE TABLE [StockExchange] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[CreatedDateTime] datetime2 NOT NULL,
	[ModifiedDateTime] datetime2 NOT NULL,
	[CreatedById] uniqueidentifier NOT NULL,
	[ModifiedById] uniqueidentifier NOT NULL,
	[Description] nvarchar(4000) NULL,
	[SortOrder] int NOT NULL default 0,
	[IsActive] bit NOT NULL default 1,
	CONSTRAINT [PK_StockExchange] PRIMARY KEY NONCLUSTERED ( [Id] )
);

GO

CREATE TABLE [Strategy] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[CreatedDateTime] datetime2 NOT NULL,
	[ModifiedDateTime] datetime2 NOT NULL,
	[CreatedById] uniqueidentifier NOT NULL,
	[ModifiedById] uniqueidentifier NOT NULL,
	[Description] nvarchar(4000) NULL,
	[SortOrder] int NOT NULL default 0,
	[IsActive] bit NOT NULL default 1,
	[StartDate] date NULL,
	[ExpiryDate] date NULL,
	CONSTRAINT [PK_Strategy] PRIMARY KEY NONCLUSTERED ( [Id] )
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

CREATE TABLE [Watchlist] (
	[Id] uniqueidentifier NOT NULL default newId(),
	[CreatedDateTime] datetime2 NOT NULL,
	[ModifiedDateTime] datetime2 NOT NULL,
	[CreatedById] uniqueidentifier NOT NULL,
	[ModifiedById] uniqueidentifier NOT NULL,
	[Name] nvarchar(4000) NOT NULL,
	[Description] nvarchar(4000) NULL,
	CONSTRAINT [PK_Watchlist] PRIMARY KEY NONCLUSTERED ( [Id] )
);

GO

CREATE TABLE [WatchlistStock] (
	[WatchlistId] uniqueidentifier NOT NULL,
	[StockId] uniqueidentifier NOT NULL,
	CONSTRAINT [PK_WatchlistStock] PRIMARY KEY NONCLUSTERED ( [WatchlistId], [StockId] )
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

CREATE INDEX [IDX_CreatedById]
ON [MarketIndex] ( [CreatedById] );

ALTER TABLE [MarketIndex]
ADD CONSTRAINT [FK_MarketIndex_User_CreatedBy]
FOREIGN KEY ( [CreatedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_ModifiedById]
ON [MarketIndex] ( [ModifiedById] );

ALTER TABLE [MarketIndex]
ADD CONSTRAINT [FK_MarketIndex_User_ModifiedBy]
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

CREATE INDEX [IDX_CreatedById]
ON [StockExchange] ( [CreatedById] );

ALTER TABLE [StockExchange]
ADD CONSTRAINT [FK_StockExchange_User_CreatedBy]
FOREIGN KEY ( [CreatedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_ModifiedById]
ON [StockExchange] ( [ModifiedById] );

ALTER TABLE [StockExchange]
ADD CONSTRAINT [FK_StockExchange_User_ModifiedBy]
FOREIGN KEY ( [ModifiedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_StockExchangeId]
ON [Stock] ( [StockExchangeId] );

ALTER TABLE [Stock]
ADD CONSTRAINT [FK_Stock_StockExchange]
FOREIGN KEY ( [StockExchangeId] ) REFERENCES [StockExchange] ( [Id] );

GO

CREATE INDEX [IDX_CreatedById]
ON [Stock] ( [CreatedById] );

ALTER TABLE [Stock]
ADD CONSTRAINT [FK_Stock_User_CreatedBy]
FOREIGN KEY ( [CreatedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_ModifiedById]
ON [Stock] ( [ModifiedById] );

ALTER TABLE [Stock]
ADD CONSTRAINT [FK_Stock_User_ModifiedBy]
FOREIGN KEY ( [ModifiedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_CreatedById]
ON [Strategy] ( [CreatedById] );

ALTER TABLE [Strategy]
ADD CONSTRAINT [FK_Strategy_User_CreatedBy]
FOREIGN KEY ( [CreatedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_ModifiedById]
ON [Strategy] ( [ModifiedById] );

ALTER TABLE [Strategy]
ADD CONSTRAINT [FK_Strategy_User_ModifiedBy]
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

CREATE INDEX [IDX_StockId]
ON [WatchlistStock] ( [StockId] );

ALTER TABLE [WatchlistStock]
ADD CONSTRAINT [FK_WatchlistStock_Stock]
FOREIGN KEY ( [StockId] ) REFERENCES [Stock] ( [Id] );

GO

CREATE INDEX [IDX_WatchlistId]
ON [WatchlistStock] ( [WatchlistId] );

ALTER TABLE [WatchlistStock]
ADD CONSTRAINT [FK_WatchlistStock_Watchlist]
FOREIGN KEY ( [WatchlistId] ) REFERENCES [Watchlist] ( [Id] ) ON DELETE CASCADE;

GO

CREATE INDEX [IDX_CreatedById]
ON [Watchlist] ( [CreatedById] );

ALTER TABLE [Watchlist]
ADD CONSTRAINT [FK_Watchlist_User_CreatedBy]
FOREIGN KEY ( [CreatedById] ) REFERENCES [User] ( [Id] );

GO

CREATE INDEX [IDX_ModifiedById]
ON [Watchlist] ( [ModifiedById] );

ALTER TABLE [Watchlist]
ADD CONSTRAINT [FK_Watchlist_User_ModifiedBy]
FOREIGN KEY ( [ModifiedById] ) REFERENCES [User] ( [Id] );

GO

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

