USE master
GO

CREATE DATABASE [{{DatabaseName}}]
ON  PRIMARY (
	NAME = N'{{DatabaseName}}'
	,FILENAME = N'D:\{{DatabaseName}}.mdf'
	,MAXSIZE = UNLIMITED)
LOG ON (
	NAME = N'Demo_log'
	,FILENAME = N'D:\{{DatabaseName}}_log.ldf'
	,MAXSIZE = 2048GB
	,FILEGROWTH = 65536KB )
GO

ALTER DATABASE [{{DatabaseName}}] SET RECOVERY SIMPLE WITH NO_WAIT
GO

ALTER DATABASE [{{DatabaseName}}] SET ALLOW_SNAPSHOT_ISOLATION ON
GO
