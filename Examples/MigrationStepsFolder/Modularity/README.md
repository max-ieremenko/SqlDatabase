Modularity upgrade
===========================

Table of Contents
-----------------

<!-- toc -->

- [Step file name](#file-name)
- [Step dependencies](#dependencies)
- [Select/update a module version](#module-version)
- [Execution](#execution)
- [SqlDatabase.exe -whatIf](#whatIf)

<!-- tocstop -->

Step file name <a name="file-name"></a>
===
A file name must be in the format "[module name]\_[version from]\_[version to].[extension]", for example

|File|Module name|Version from|Version to|Extension|
|:--|:----------|:----------|:----------|:----------|
|moduleA_1.0_2.0.sql|moduleA|1.0|2.0|.sql (text file with sql scripts)|
|moduleA_2.1_2.2.exe|moduleA|2.1|2.2|.exe (.net assembly with a script implementation)|
|moduleB_1.0_2.0.dll|moduleB|1.0|2.0|.dll (.net assembly with a script implementation)|
|moduleB_2.0_2.1.ps1|moduleB|2.0|2.1|.ps1 (text file with powershell script)|

[Back to ToC](#table-of-contents)

Step`s dependencies <a name="dependencies"></a>
===
Dependencies are optional. They are written as a special type of comment in the form "module dependency: [module name] [module version]". Module names case-insensitive.

#### MSSQL Server .sql step

Once `GO` instruction has been processed, SqlDatabase no longer looks for dependencies. Therefore, all dependencies must be at the very top of a script.

The following step depends on module A version 2.0 and module B version 1.0.

```sql
/*
* module dependency: a 2.0
* module dependency: b 1.0
*/
GO
...
```

The following step depends on module A version 2.0.

```sql
-- module dependency: a 2.0
GO
...
```

The following example is invalid:

```sql
/*
module dependency: a 2.0
*/
GO
...
```

#### PostgreSQL and MySQL .sql step

Once a line `;` has been processed, SqlDatabase no longer looks for dependencies. Therefore, all dependencies must be at the very top of a script.

The following step depends on module A version 2.0 and module B version 1.0.

```sql
/*
* module dependency: a 2.0
* module dependency: b 1.0
*/
;
...
```

The following step depends on module A version 2.0.

```sql
-- module dependency: a 2.0
;
...
```

The following example is invalid:

```sql
/*
module dependency: a 2.0
*/
;
...
```

#### .dll, .exe or .ps1 step
Dependencies for this step can be defined in a separate text file with the same name. The text file must be the next to step file.

Step moduleA_2.1_2.2.exe, text file moduleA_2.1_2.2.txt

```txt
/*
* module dependency: b 2.0
* module dependency: c 1.0
*/
...
```

Step moduleB_1.0_2.0.dll, text file moduleB_1.0_2.0.txt

```txt
-- module dependency: b 2.0
-- module dependency: c 1.0
...
```

[Back to ToC](#table-of-contents)

Select/update a module version <a name="module-version"></a>
===
Scripts for resolving and updating a module version are defined in the [configuration file](../../ConfigurationFile).

Default scripts must be changed in order to support modularity. Few examples:

#### MSSQL Server, store versions in the database properties

```sql
-- configuration: select current version
SELECT value from sys.fn_listextendedproperty('version-{{ModuleName}}', default, default, default, default, default, default)

-- configuration: update current version
EXEC sys.sp_updateextendedproperty @name=N'version-{{ModuleName}}', @value=N'{{TargetVersion}}'
```

#### MSSQL Server, store versions in a specific table

```sql
-- a table
CREATE TABLE dbo.Version
(
    ModuleName NVARCHAR(100) NOT NULL
    , Version NVARCHAR(20) NOT NULL
)
GO
ALTER TABLE dbo.Version ADD CONSTRAINT PK_dbo_Version PRIMARY KEY CLUSTERED (ModuleName);

-- configuration: select current version
SELECT Version FROM dbo.Version WHERE ModuleName = N'{{ModuleName}}'

-- configuration: update current version
UPDATE dbo.Version SET Version = N'{{TargetVersion}}' WHERE ModuleName = N'{{ModuleName}}'
```

#### PostgreSQL, store versions in a specific table

```sql
-- a table
CREATE TABLE public.version
(
    module_name public.citext NOT NULL
    , version varchar(20) NOT NULL
);

ALTER TABLE public.version ADD CONSTRAINT pk_version PRIMARY KEY (module_name);

-- configuration: select current version
SELECT version FROM public.version WHERE module_name = '{{ModuleName}}'

-- configuration: update current version
UPDATE public.version SET version = '{{TargetVersion}}' WHERE module_name = '{{ModuleName}}'
```

Warn: SqlDatabase does not validate the provided script, please make sure that script is working before running SqlDatabase.

#### MySQL, store versions in a specific table

```sql
-- a table
CREATE TABLE version
(
	module_name VARCHAR(20) NOT NULL
	,version VARCHAR(20) NOT NULL
);

ALTER TABLE version ADD CONSTRAINT pk_version PRIMARY KEY (module_name);

-- configuration: select current version
SELECT version FROM version WHERE module_name = '{{ModuleName}}'

-- configuration: update current version
UPDATE version SET version = '{{TargetVersion}}' WHERE module_name = '{{ModuleName}}'
```

Warn: SqlDatabase does not validate the provided script, please make sure that script is working before running SqlDatabase.

[Back to ToC](#table-of-contents)

Execution
===
In the current folder there are migration steps for three modules *person*, *book* and *reader*.

- [person_1.0_2.0.sql](person_1.0_2.0.sql) - creates a table "dbo.Person"
- [book_1.0_2.0.sql](book_1.0_2.0.sql) - creates a table "dbo.Book", depends on person 2.0
- [reader_1.0_2.0.sql](reader_1.0_2.0.sql) - creates a table "dbo.BookComment", depends on person 2.0 and book 2.0

The folder structure does not matter, SqlDatabase analyzes all files and folders recursively.

#### runtime

1. Load all migration steps
2. Resolve the current version of modules: imagine author is 1.0, book is 1.0 and reader is 1.0
3. Resolve migration step dependencies
4. Build migration sequence: person 1.0 => 2.0; book 1.0 => 2.0; reader 1.0 => 2.0;
5. Execute each step one by one:

```sql
/* person 1.0 => 2.0 */
execute person_1.0_2.0.sql
execute "update person module version to 2.0"
execute "check that person module version is 2.0"

/* book 1.0 => 2.0 */
execute book_1.0_2.0.sql
execute "update book module version to 2.0"
execute "check that book module version is 2.0"

/* reader 1.0 => 2.0 */
execute reader_1.0_2.0.sql
execute "update reader module version to 2.0"
execute "check that reader module version is 2.0"
```

-whatIf option <a name="whatIf"></a>
===
Use SqlDatabase -whatIf to list all steps with resolved dependencies and to test upgrade sequence.

[Back to ToC](#table-of-contents)
