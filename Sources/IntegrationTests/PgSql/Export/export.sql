SELECT person.id
       ,person.name || person.second_name AS name
	   ,address.city
FROM demo.person person
LEFT JOIN demo.person_address address ON person.id = address.person_id