INSERT INTO person_address(person_id, city)
SELECT person.id, '{{JohnCity}}'
FROM person person
WHERE person.Name = 'John';

INSERT INTO person_address(person_id, city)
SELECT person.id, '{{MariaCity}}'
FROM person person
WHERE person.name = 'Maria';

SELECT * FROM person_address;