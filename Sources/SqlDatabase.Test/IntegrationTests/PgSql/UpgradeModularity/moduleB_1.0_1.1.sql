-- module dependency: moduleA 2.0
;

CREATE SCHEMA module_b;

CREATE TABLE module_b.person_address
(
	id serial
	,person_id integer NOT NULL
	,city varchar(250) NOT NULL
);

ALTER TABLE module_b.person_address ADD CONSTRAINT pk_module_b_person_address PRIMARY KEY (id);

ALTER TABLE module_b.person_address ADD CONSTRAINT fk_module_b_person_address_person FOREIGN KEY (person_id) REFERENCES module_a.person (id);
