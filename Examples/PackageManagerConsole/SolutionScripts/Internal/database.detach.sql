USE master
GO

ALTER DATABASE [$(dbname)] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
GO

EXEC master.dbo.sp_detach_db @dbname = N'$(dbname)'
GO
