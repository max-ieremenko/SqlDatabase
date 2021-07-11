SELECT person.id
       ,concat(person.name, ' ', person.second_name) AS name
	   ,address.city
FROM person person
LEFT JOIN person_address address ON person.id = address.person_id