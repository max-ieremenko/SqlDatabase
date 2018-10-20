INSERT INTO demo.PersonAddress(PersonId, City)
SELECT Person.Id, '{{JohnCity}}'
FROM demo.Person Person
WHERE Person.Name = 'John'
GO

INSERT INTO demo.PersonAddress(PersonId, City)
SELECT Person.Id, '{{MariaCity}}'
FROM demo.Person Person
WHERE Person.Name = 'Maria'
GO
