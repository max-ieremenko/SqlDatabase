INSERT INTO demo.Department(Name)
VALUES (N'HR'), (N'Dev'), (N'Test'), (N'S&S')
GO

WITH employee AS
(
SELECT N'' FullName, N'' Department WHERE 1=0
UNION ALL
SELECT N'Anna', N'HR'
UNION ALL
SELECT N'David', N'DEV'
UNION ALL
SELECT N'Georg', N'Test'
UNION ALL
SELECT N'Ivan', N'S&S'
)

INSERT INTO demo.Employee(FullName, DepartmentId)
SELECT employee.FullName
       ,Department.Id
FROM employee
     LEFT JOIN demo.Department Department ON employee.Department = Department.Name
GO