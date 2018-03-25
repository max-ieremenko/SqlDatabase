CREATE TABLE demo.Employee
(
	Id INT NOT NULL IDENTITY(1, 1)
	,FullName NVARCHAR(300) NOT NULL
	,DepartmentId INT NOT NULL
)
GO

ALTER TABLE demo.Employee ADD CONSTRAINT PK_Employee PRIMARY KEY CLUSTERED (Id)
GO

CREATE NONCLUSTERED INDEX IX_Employee_DepartmentId ON demo.Employee	(DepartmentId)
GO

CREATE NONCLUSTERED INDEX IX_Employee_FullName ON demo.Employee	(FullName)
GO

ALTER TABLE demo.Employee ADD CONSTRAINT FK_Employee_Department
FOREIGN KEY	(DepartmentId)
REFERENCES demo.Department (Id)
ON UPDATE NO ACTION 
ON DELETE NO ACTION 
GO
