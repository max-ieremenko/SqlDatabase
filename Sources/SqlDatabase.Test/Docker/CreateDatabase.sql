USE master
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