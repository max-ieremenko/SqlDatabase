Some tests require already existing database on SqlServer.

Use follwing script to create it:
```sql
USE master
GO

CREATE DATABASE SqlDatabaseTest
 ON PRIMARY 
( NAME = N'SqlDatabaseTest', FILENAME = N'D:\Work\SqlDatabase\Data\SqlDatabaseTest.mdf' )
 LOG ON 
( NAME = N'SqlDatabaseTest_log', FILENAME = N'D:\Work\SqlDatabase\Data\SqlDatabaseTest_log.ldf')
GO

ALTER DATABASE SqlDatabaseTest SET RECOVERY SIMPLE WITH NO_WAIT
GO

EXEC SqlDatabaseTest.sys.sp_addextendedproperty @name=N'version', @value=N'1.0'
GO
```

And check the connection string in app.config.