CREATE TABLE demo.person
(
	id serial
	,name varchar(250) NOT NULL
);

ALTER TABLE demo.person ADD CONSTRAINT pk_demo_person PRIMARY KEY (id);
