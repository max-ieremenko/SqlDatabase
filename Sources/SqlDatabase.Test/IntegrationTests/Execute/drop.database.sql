USE master
GO

EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{{DbName}}'
GO

ALTER DATABASE [{{DbName}}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

PRINT 'drop {{DbName}}'
DROP DATABASE [{{DbName}}]
GO

