UPDATE demo.Person
SET SecondName = '{{JohnSecondName}}'
WHERE Name = 'John'
GO

UPDATE demo.Person
SET SecondName = '{{MariaSecondName}}'
WHERE Name = 'Maria'
GO
