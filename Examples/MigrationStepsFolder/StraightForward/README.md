Straight forward upgrade
===========================

Step file name <a name="file-name"></a>
===
A file name must be in the format "[version from]\_[version to].[extension]", for example

|File|Version from|Version to|Extension|
|:--|:----------|:----------|:----------|
|1.0_2.0.sql|1.0|2.0|.sql (text file with sql scripts)|
|2.0_2.1.exe|2.0|2.1|.exe (.net assembly with a script implementation)|
|2.1_2.2.dll|2.1|2.2|.dll (.net assembly with a script implementation)|
|2.2_3.0.ps1|2.2|3.0|.ps1 (text file with powershell script)|

Select/update a database version <a name="version"></a>
===
Scripts for resolving and updating a database version are defined in the [configuration file](../../ConfigurationFile).

#### MSSQL Server, store versions in the database properties (default)

```sql
-- select current version
SELECT value FROM sys.fn_listextendedproperty('version', default, default, default, default, default, default)

-- update current version
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'{{TargetVersion}}'
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

-- select current version
SELECT Version FROM dbo.Version WHERE ModuleName = N'database'

-- update current version
UPDATE dbo.Version SET Version = N'{{TargetVersion}}' WHERE ModuleName = N'database'
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

-- select current version
SELECT version FROM public.version WHERE module_name = 'database'

-- update current version
UPDATE public.version SET version = '{{TargetVersion}}' WHERE module_name = 'database'
```

Execution
===
In the current folder are migration steps
- [1.0_1.1.sql](1.0_1.1.sql) - creates a table "dbo.Person"
- [1.1_2.0.sql](1.1_2.0.sql) - creates a table "dbo.Book"
- [2.0_3.0.sql](2.0_3.0.sql) - creates a table "dbo.BookComment"

The folder structure does not matter, SqlDatabase analyzes all files and folders recursively.

#### runtime
1. Load all migartion steps
2. Resolve the current version of database: 1.0
4. build migration sequence: 1.0 => 1.1; 1.1 => 2.0; 2.0 => 3.0
5. Execute each step one by one:

```sql
/* 1.0 => 1.1 */
execute 1.0_1.1.sql
execute "update database version to 1.1"
execute "check that current version is 1.1"

/* 1.1 => 2.0 */
execute 1.1_2.0.sql
execute "update database version to 2.0"
execute "check that current version is 2.0"

/* 2.0 => 3.0 */
execute 2.0_3.0.sql
execute "update database version to 3.0"
execute "check that current version is 3.0"
```

-whatIf option <a name="whatIf"></a>
===
Use SqlDatabase -whatIf to list all steps and test upgrade sequence.
