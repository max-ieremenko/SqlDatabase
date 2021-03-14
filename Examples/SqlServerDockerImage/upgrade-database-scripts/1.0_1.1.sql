ALTER TABLE demo.Employee ADD Birthday DATETIME2 NULL
GO

UPDATE demo.Employee
SET Birthday = DATEADD(year, -20, GETDATE())
WHERE FullName = 'Anna'

UPDATE demo.Employee
SET Birthday = DATEADD(year, -22, GETDATE())
WHERE FullName = 'David'

UPDATE demo.Employee
SET Birthday = DATEADD(year, -30, GETDATE())
WHERE FullName = 'Georg'

UPDATE demo.Employee
SET Birthday = DATEADD(year, -40, GETDATE())
WHERE FullName = 'Ivan'
GO

ALTER TABLE demo.Employee ALTER COLUMN Birthday DATETIME2 NULL