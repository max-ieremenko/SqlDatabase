USE master
GO

EXEC sys.sp_configure N'show advanced options', N'1'  RECONFIGURE WITH OVERRIDE
GO
EXEC sys.sp_configure N'max server memory (MB)', N'512'
GO
RECONFIGURE WITH OVERRIDE
GO
EXEC sys.sp_configure N'show advanced options', N'0'  RECONFIGURE WITH OVERRIDE
GO

PRINT 'create database...'
CREATE DATABASE SqlDatabaseTest
GO

PRINT 'change recovery model...'
ALTER DATABASE SqlDatabaseTest SET RECOVERY SIMPLE WITH NO_WAIT
GO

PRINT 'set database version...'
EXEC SqlDatabaseTest.sys.sp_addextendedproperty @name=N'version', @value=N'1.0'
GO

PRINT 'set module version...'
EXEC SqlDatabaseTest.sys.sp_addextendedproperty @name=N'version-SomeModuleName', @value=N'2.0'
GO