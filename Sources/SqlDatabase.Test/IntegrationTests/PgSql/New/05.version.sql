CREATE TABLE public.version
(
	module_name public.citext NOT NULL
	,version varchar(20) NOT NULL
);

ALTER TABLE public.version
    ADD CONSTRAINT pk_version PRIMARY KEY (module_name);

INSERT INTO public.version (module_name, version) VALUES
('database', '1.2')
,('ModuleA', '1.0')
,('ModuleB', '1.0')
,('ModuleC', '1.0');
