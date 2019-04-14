IF (OBJECT_ID('dbo.ExportTest') IS NOT NULL) BEGIN
	DROP TABLE dbo.ExportTest
END
GO

CREATE TABLE dbo.ExportTest
(
	Id INT NOT NULL IDENTITY(1, 1),
	ColTinyInt TINYINT,
	ColSmallInt SMALLINT,
	ColBigInt BIGINT,
	ColBit BIT,
	ColMoneyDefault MONEY,
	ColMoney MONEY,
	ColNVarchar NVARCHAR(100),
	CONSTRAINT [PK_ExportTest] PRIMARY KEY CLUSTERED (Id ASC)
)
GO

INSERT INTO dbo.ExportTest(ColNVarchar)
VALUES ('NVarchar 1')
      ,(NULL)
GO
