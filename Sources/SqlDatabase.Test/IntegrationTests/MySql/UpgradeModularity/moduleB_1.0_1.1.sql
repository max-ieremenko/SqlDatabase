-- module dependency: moduleA 2.0

CREATE TABLE module_b_person_address
(
	id INT NOT NULL AUTO_INCREMENT
	,person_id INT NOT NULL
	,city VARCHAR(250) NOT NULL
	,PRIMARY KEY pk_module_b_person_address (id)
);

ALTER TABLE module_b_person_address ADD CONSTRAINT fk_module_b_person_address_person FOREIGN KEY (person_id) REFERENCES module_a_person (id);
