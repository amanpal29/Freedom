IF NOT EXISTS(SELECT * FROM master.dbo.syslogins WHERE name='freedomserver')
CREATE LOGIN [freedomserver] WITH PASSWORD=N'my_freedom', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
CREATE USER [freedomserver] FOR LOGIN [freedomserver]
GO
ALTER USER [freedomserver] WITH DEFAULT_SCHEMA=[dbo]
GO
CREATE ROLE db_executor
GO
GRANT EXECUTE ON SCHEMA::dbo TO db_executor
GO
EXEC sp_addrolemember N'db_datareader', N'freedomserver'
GO
EXEC sp_addrolemember N'db_datawriter', N'freedomserver'
GO
EXEC sp_addrolemember N'db_executor', N'freedomserver'
GO
