﻿CREATE ROLE adminuser WITH
  LOGIN
  NOSUPERUSER
  INHERIT
  CREATEDB
  CREATEROLE
  NOREPLICATION
  PASSWORD 'qwerty';
  
SET ROLE adminuser;

CREATE DATABASE sqldatabasetest;

\connect sqldatabasetest;

SET ROLE adminuser;

CREATE EXTENSION citext;

CREATE TABLE public.version
(
	module_name public.citext NOT NULL
	,version varchar(20) NOT NULL
);

CREATE TABLE public.version2
(
	module_name public.citext NOT NULL
	,version varchar(20) NOT NULL
);

ALTER TABLE public.version
    ADD CONSTRAINT pk_version PRIMARY KEY (module_name);

INSERT INTO public.version (module_name, version) VALUES
('database', '1.0')
,('SomeModuleName', '2.0');


CREATE TYPE public.mood AS ENUM ('sad', 'ok', 'happy');

CREATE TYPE public.inventory_item AS (
    name            text,
    supplier_id     integer,
    price           numeric
);
