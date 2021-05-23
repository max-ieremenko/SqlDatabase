CREATE SCHEMA moduleA
GO

CREATE TABLE moduleA.Person
(
	Id INT NOT NULL IDENTITY(1, 1)
	,Name NVARCHAR(250) NOT NULL
)
GO

ALTER TABLE moduleA.Person ADD CONSTRAINT PK_moduleA_Person PRIMARY KEY CLUSTERED (Id)
GO
