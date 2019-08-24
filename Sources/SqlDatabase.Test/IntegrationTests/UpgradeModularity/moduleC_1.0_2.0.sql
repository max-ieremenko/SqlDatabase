-- module dependency: moduleA 2.0
-- module dependency: moduleB 1.1
GO

INSERT INTO moduleA.Person(Name)
VALUES ('John'), ('Maria')
GO

INSERT INTO moduleB.PersonAddress(PersonId, City)
SELECT Person.Id, 'London'
FROM demo.Person Person
WHERE Person.Name = 'John'
GO
