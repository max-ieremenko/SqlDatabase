/*
* module dependency: person 1.0
* module dependency: book 1.0
*/
GO

CREATE TABLE dbo.BookComment
(
	Id INT NOT NULL IDENTITY(1, 1)
	,ReaderId INT NOT NULL
	,BookId INT NOT NULL
	,Comment NVARCHAR(MAX) NOT NULL
)

ALTER TABLE dbo.BookComment ADD CONSTRAINT PK_dbo_BookComment PRIMARY KEY CLUSTERED (Id)
GO

ALTER TABLE dbo.BookComment ADD CONSTRAINT FK_Book_ReaderId FOREIGN KEY (ReaderId) REFERENCES dbo.Person (Id)
GO

ALTER TABLE dbo.BookComment ADD CONSTRAINT FK_Book_BookId FOREIGN KEY (BookId) REFERENCES dbo.Book (Id)
GO