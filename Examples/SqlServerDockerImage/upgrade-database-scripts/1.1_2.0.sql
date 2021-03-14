ALTER TABLE demo.Employee ADD ManagerId INT NULL
GO

ALTER TABLE demo.Employee ADD CONSTRAINT FK_demo_Employee_ManagerId FOREIGN KEY (ManagerId) REFERENCES demo.Employee (Id)
GO

CREATE NONCLUSTERED INDEX IX_Employee_ManagerId ON demo.Employee (ManagerId)
GO
