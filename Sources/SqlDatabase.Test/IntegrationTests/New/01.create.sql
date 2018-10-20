USE master
GO

EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{{DbName}}'
GO

IF DB_ID('{{DbName}}') IS NOT NULL BEGIN
	ALTER DATABASE [{{DbName}}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
END
GO

IF DB_ID('{{DbName}}') IS NOT NULL BEGIN
	PRINT 'drop {{DbName}}'
	DROP DATABASE [{{DbName}}]
END
GO

CREATE DATABASE [{{DbName}}]
GO

ALTER DATABASE [{{DbName}}] SET RECOVERY SIMPLE
GO

