CREATE DATABASE sqldatabasetest;

USE sqldatabasetest;

CREATE TABLE version
(
	module_name varchar(20) NOT NULL
	,version varchar(20) NOT NULL
);

ALTER TABLE version
    ADD CONSTRAINT pk_version PRIMARY KEY (module_name);

INSERT INTO version (module_name, version) VALUES
('database', '1.0')
,('SomeModuleName', '2.0');
