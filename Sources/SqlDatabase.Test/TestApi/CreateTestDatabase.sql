CREATE DATABASE SqlDatabaseTest
GO

ALTER DATABASE SqlDatabaseTest SET RECOVERY SIMPLE WITH NO_WAIT
GO

EXEC SqlDatabaseTest.sys.sp_addextendedproperty @name=N'version', @value=N'1.0'
GO