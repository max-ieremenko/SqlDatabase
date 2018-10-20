CREATE TABLE demo.PersonAddress
(
	Id INT NOT NULL IDENTITY(1, 1)
	,PersonId INT NOT NULL
	,City NVARCHAR(250) NOT NULL
)
GO

ALTER TABLE demo.PersonAddress ADD CONSTRAINT PK_demo_PersonAddress PRIMARY KEY CLUSTERED (Id)
GO

ALTER TABLE demo.PersonAddress ADD CONSTRAINT FK_PersonAddress_Person FOREIGN KEY (PersonId) REFERENCES demo.Person (Id)
GO
