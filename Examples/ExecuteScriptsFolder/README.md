Execute script(s) (file)
=================

```bash
$ SqlDatabase execute ^
      "-database=Data Source=server;Initial Catalog=database;Integrated Security=True" ^
      -from=c:\SqlDatabase\Examples\ExecuteScriptsFolder ^
      -varVariable1=value1 ^
      -varVariable2=value2

PS> Execute-SqlDatabase `
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True" `
      -from c:\SqlDatabase\Examples\ExecuteScriptsFolder `
      -var Variable1=value1,Variable2=value2 `
      -InformationAction Continue
```

execute script from folder "c:\SqlDatabase\Examples\ExecuteScriptsFolder" on *[MyDatabase]* on server *[MyServer]* with "Variable1=value1" and "Variable2=value2"

CLI
===

|Option|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|a path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.|
|-fromSql|an sql script text. Repeat -fromSql to setup several scripts.|
|-configuration|a path to application [configuration file](../ConfigurationFile).|
|-log|optional path to log file|
|-var|set a variable in format "=var[name of variable]=[value of variable]"|
|-whatIf|shows what would happen if the command runs. The command is not run|

#### -from

```bash
# execute migration script files in Scripts folder
-from=C:\MyDatabase\Scripts

# execute script files from Scripts.zip archive
-from=C:\MyDatabase\Scripts.zip

# execute script files from Scripts folder in MyDatabase.zip archive
-from=C:\MyDatabase.zip\Scripts

# execute scripts from file Script.sql
-from=C:\MyDatabase\Script.sql

# execute scripts from file Script.sql in MyDatabase.zip archive
-from=C:\MyDatabase.zip\Script.sql
```

#### -fromSql

```bash
"-fromSql=CREATE TABLE [dbo].[Person]"

"-fromSql=CREATE TABLE [{{Schema}}].[{{Table}}]" -varSchema=dbo -varTable=Person
```

#### -var

```sql
-- script.sql
PRINT 'create table {{Schema}}.{{Table}}'
CREATE TABLE [{{Schema}}].[{{Table}}]
```

```bash
# execute script.sql
-from=script.sql -varSchema=dbo -varTable=Person

# output
script.sql ...
   variable Schema was replaced with dbo
   variable Table was replaced with Person
```

```sql
-- script at runtime
PRINT 'create table dbo.Person'
CREATE TABLE [dbo].[Person]
```

#### Exit codes
* 0 - OK
* 1 - invalid command line
* 2 - errors during execution

Script`s execution order
========================

1. run all script`s files in the root folder, sorted alphabetically
2. run all script`s in each sub-folder, sorted alphabetically

|File|Execution order|
|:--|:----------|
|data|4|
|└── 01_staff.sql|4.1|
|01_demo.sql|1|
|02_demo.Department.sql|2|
|03_demo.Employee.sql|3|

Predefined variables
========================

|Name|Description|
|:--|:----------|
|DatabaseName|the target database name|

MSSQL Server script example
=============================

File name 02_demo.Department.sql

```sql
PRINT 'create table demo.Department'
GO

CREATE TABLE demo.Department
(
	Id INT NOT NULL IDENTITY(1, 1)
	,Name NVARCHAR(300) NOT NULL
)
GO

ALTER TABLE demo.Department ADD CONSTRAINT PK_Department PRIMARY KEY CLUSTERED (Id)
GO

CREATE NONCLUSTERED INDEX IX_Department_Name ON demo.Department	(Name)
GO
```

PostgreSQL script example
=============================

```sql
DO $$
BEGIN
RAISE NOTICE 'create table demo.department';
END
$$;

CREATE TABLE demo.department
(
	id serial
	,name varchar(300) NOT NULL
);

ALTER TABLE demo.department ADD CONSTRAINT pk_department PRIMARY KEY (Id);

CREATE INDEX ix_department_name ON demo.department (name);
```

.ps1 script example
=============================

File name 02_demo.Department.ps1, see details [here](../PowerShellScript).

```powershell
param (
    $Command,
    $Variables
)

Write-Information "create table demo.Department"

$Command.CommandText = @"
CREATE TABLE demo.Department
(
	Id INT NOT NULL IDENTITY(1, 1)
	,Name NVARCHAR(300) NOT NULL
)
"@
$Command.ExecuteNonQuery()

$Command.CommandText = "ALTER TABLE demo.Department ADD CONSTRAINT PK_Department PRIMARY KEY CLUSTERED (Id)"
$Command.ExecuteNonQuery()

$Command.CommandText = "CREATE NONCLUSTERED INDEX IX_Department_Name ON demo.Department	(Name)"
$Command.ExecuteNonQuery()
```

Assembly script example
=======================

File name 02_demo.Department.dll, see details [here](../CSharpMirationStep).

```C#
namespace <any namespace name>
{
    public /*sealed*/ class SqlDatabaseScript /*: IDisposable*/
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            Console.WriteLine("create table demo.Department");

            command.CommandText = @"
CREATE TABLE demo.Department
(
	Id INT NOT NULL IDENTITY(1, 1)
	,Name NVARCHAR(300) NOT NULL
)
            ";
            command.ExecuteNonQuery();
            
            command.CommandText = 'ALTER TABLE demo.Department ADD CONSTRAINT PK_Department PRIMARY KEY CLUSTERED (Id)';
            command.ExecuteNonQuery();
            
            command.CommandText = 'CREATE NONCLUSTERED INDEX IX_Department_Name ON demo.Department	(Name)';
            command.ExecuteNonQuery();
        }
    }
}
```