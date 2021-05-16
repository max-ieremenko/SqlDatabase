CREATE TABLE demo.person_address
(
	id serial
	,person_id integer NOT NULL
	,city varchar(250) NOT NULL
);

ALTER TABLE demo.person_address ADD CONSTRAINT pk_demo_person_address PRIMARY KEY (id);

ALTER TABLE demo.person_address ADD CONSTRAINT fk_demo_person_address_person FOREIGN KEY (person_id) REFERENCES demo.person (id);
