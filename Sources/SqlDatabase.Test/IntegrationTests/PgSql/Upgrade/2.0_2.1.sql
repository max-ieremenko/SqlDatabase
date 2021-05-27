UPDATE demo.person
SET second_name = '{{JohnSecondName}}'
WHERE name = 'John';

UPDATE demo.person
SET second_name = '{{MariaSecondName}}'
WHERE name = 'Maria';

SELECT * FROM demo.person;