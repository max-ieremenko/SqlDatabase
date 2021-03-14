USE master
GO

EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{{DatabaseName}}'
GO

IF EXISTS(SELECT * FROM sys.databases WHERE Name=N'{{DatabaseName}}') BEGIN
	ALTER DATABASE [{{DatabaseName}}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE
END
GO

IF EXISTS(SELECT * FROM sys.databases WHERE Name=N'{{DatabaseName}}') BEGIN
	DROP DATABASE [{{DatabaseName}}]
END
GO