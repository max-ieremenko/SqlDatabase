-- module dependency: moduleA 2.0
GO

CREATE SCHEMA moduleB
GO

CREATE TABLE moduleB.PersonAddress
(
	Id INT NOT NULL IDENTITY(1, 1)
	,PersonId INT NOT NULL
	,City NVARCHAR(250) NOT NULL
)
GO

ALTER TABLE moduleB.PersonAddress ADD CONSTRAINT PK_moduleB_PersonAddress PRIMARY KEY CLUSTERED (Id)
GO

ALTER TABLE moduleB.PersonAddress ADD CONSTRAINT FK_PersonAddress_Person FOREIGN KEY (PersonId) REFERENCES moduleA.Person (Id)
GO
