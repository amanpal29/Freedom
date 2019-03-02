IF NOT EXISTS(SELECT * FROM sys.database_principals WHERE name='freedomserver')
CREATE USER [freedomserver] WITH PASSWORD=N'My_Freedom', DEFAULT_SCHEMA=[dbo]; 
GO
ALTER ROLE db_datareader ADD MEMBER [freedomserver]; 
GO
ALTER ROLE db_datawriter ADD MEMBER [freedomserver]; 
GO
