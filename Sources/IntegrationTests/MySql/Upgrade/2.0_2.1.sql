UPDATE person
SET second_name = '{{JohnSecondName}}'
WHERE name = 'John';

UPDATE person
SET second_name = '{{MariaSecondName}}'
WHERE name = 'Maria';

SELECT * FROM person;