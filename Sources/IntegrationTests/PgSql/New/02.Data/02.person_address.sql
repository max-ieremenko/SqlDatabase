INSERT INTO demo.person_address(person_id, city)
SELECT person.id, '{{JohnCity}}'
FROM demo.person person
WHERE person.Name = 'John';

INSERT INTO demo.person_address(person_id, city)
SELECT person.id, '{{MariaCity}}'
FROM demo.person person
WHERE person.name = 'Maria';

SELECT * FROM demo.person_address;