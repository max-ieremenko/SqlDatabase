SELECT Person.Id
       ,Person.Name + Person.SecondName Name
	   ,Address.City
FROM demo.Person Person
LEFT JOIN demo.PersonAddress Address ON Person.Id = Address.PersonId