CREATE SCHEMA module_a;

CREATE TABLE module_a.person
(
	id serial
	,name varchar(250) NOT NULL
);

ALTER TABLE module_a.person ADD CONSTRAINT pk_module_a_person PRIMARY KEY (id);

