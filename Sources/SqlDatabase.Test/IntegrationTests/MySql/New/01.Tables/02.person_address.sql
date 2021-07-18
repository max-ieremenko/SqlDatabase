CREATE TABLE person_address
(
	id INT NOT NULL AUTO_INCREMENT
	,person_id INT NOT NULL
	,city VARCHAR(250) NOT NULL
	,PRIMARY KEY pk_person_address (id)
);

ALTER TABLE person_address ADD CONSTRAINT fk_person_address_person FOREIGN KEY (person_id) REFERENCES person (id);
