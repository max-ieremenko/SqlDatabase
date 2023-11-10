CREATE TABLE version
(
	module_name VARCHAR(20) NOT NULL
	,version VARCHAR(20) NOT NULL
);

ALTER TABLE version ADD CONSTRAINT pk_version PRIMARY KEY (module_name);

INSERT INTO version (module_name, version) VALUES
('database', '1.2')
,('ModuleA', '1.0')
,('ModuleB', '1.0')
,('ModuleC', '1.0');
