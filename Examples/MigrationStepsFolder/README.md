
SqlDatabase supports [straight forward upgrade](StraightForward) and [modularity upgrade](Modularity).

Upgrade an existing database
===========================

```bash
$ SqlDatabase upgrade ^
      "-database=Data Source=server;Initial Catalog=MyDatabase;Integrated Security=True" ^
      -from=Examples\MigrationStepsFolder ^
      -varVariable1=value1 ^
      -varVariable2=value2

PS> Upgrade-SqlDatabase `
      -database "Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True" `
      -from Examples\MigrationStepsFolder `
      -var Variable1=value1,Variable2=value2
```
upgrade existing database *MyDatabase* on Sql Server *MyServer* based on scripts from *Examples\MigrationStepsFolder* with "Variable1=value1" and "Variable2=value2"

CLI
===

|Option|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|a path to a folder or zip archive with migration steps. Repeat -from to setup several sources.|
|-transaction|set transaction mode (none, perStep). Option [none] is default, means no transactions. Option [perStep] means to use one transaction per each migration step|
|-configuration|a path to application [configuration file](../ConfigurationFile).|
|-log|optional path to log file|
|-var|set a variable in format "=var[name of variable]=[value of variable]"|
|-whatIf|shows what would happen if the command runs. The command is not run|

#### -from

```bash
# execute migration steps from UpgradeScripts folder
-from=C:\MyDatabase\UpgradeScripts

# execute migration steps from UpgradeScripts.zip archive
-from=C:\MyDatabase\UpgradeScripts.zip

# execute migration steps from UpgradeScripts folder in MyDatabase.zip archive
-from=C:\MyDatabase.zip\UpgradeScripts
```

#### -var

```sql
-- X.X_X.Y.sql
PRINT 'drop table {{Schema}}.{{Table}}'
DROP TABLE [{{Schema}}].[{{Table}}]
```

```bash
# execute X.X_X.Y.sql
-from=script.sql -varSchema=dbo -varTable=Person

# output
script.sql ...
   variable Schema was replaced with dbo
   variable Table was replaced with Person
```

```sql
-- script at runtime
PRINT 'drop table dbo.Person'
DROP TABLE [dbo].[Person]
```

#### Exit codes
* 0 - OK
* 1 - invalid command line
* 2 - errors during execution

Step`s execution order
===============

1. Resolve the current database version
2. Build migration steps sequence
3. Execute migration steps one by one and update current database version

The folder structure does not matter, SqlDatabase analyzes all files and folders recursively.

See example of straight forward upgrade [here](StraightForward).

See example of modularity upgrade here [here](Modularity).

Predefined variables
====================

|Name|Description|
|:--|:----------|
|DatabaseName|the target database name|
|CurrentVersion|the database version before execution of current migration step|
|TargetVersion|the database version after execution of current migration step|
|ModuleName|the module name of current migration step, empty string in case of straight forward upgrade|

Opening a connection
========================

Before starting any step SqlDatabase checks if a database, provided in the connection string, exists. If database does not exists the connection will be targeted to `master` for MSSQL and `postgres` for PostgreSQL.

Migration MSSQL Server .sql step example
=============================

File name 2.0_2.1.sql

```sql
PRINT 'create table Demo'
GO

CREATE TABLE dbo.Demo
(
	Id INT NOT NULL
)
GO

PRINT 'create primary key PK_Demo'
GO

ALTER TABLE dbo.Demo ADD CONSTRAINT PK_Demo PRIMARY KEY CLUSTERED (Id)
GO
```

Migration PostgreSQL .sql step example
=============================

```sql
DO $$
BEGIN
RAISE NOTICE 'create table demo';
END
$$;

CREATE TABLE public.demo
(
	id integer NOT NULL
);

DO $$
BEGIN
RAISE NOTICE 'create primary key pk_demo';
END
$$;

ALTER TABLE public.demo ADD CONSTRAINT pk_demo PRIMARY KEY (id);
```

Migration .ps1 step example
=============================

File name 2.0_2.1.ps1, see details about powershell scripts [here](../PowerShellScript).

```powershell
param (
    $Command,
    $Variables
)

Write-Information "create table Demo"

$Command.CommandText = @"
CREATE TABLE dbo.Demo
(
	Id INT NOT NULL
)
"@
$Command.ExecuteNonQuery()

Write-Information "create primary key PK_Demo"

$Command.CommandText = "ALTER TABLE dbo.Demo ADD CONSTRAINT PK_Demo PRIMARY KEY CLUSTERED (Id)"
$Command.ExecuteNonQuery()

```

Migration .dll step example
=======================

File name 2.1_2.2.dll, see details about assembly scripts [here](../CSharpMirationStep).

```C#
namespace <any namespace name>
{
    public /*sealed*/ class SqlDatabaseScript /*: IDisposable*/
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            Console.WriteLine("create table Demo");

            command.CommandText = @"
CREATE TABLE dbo.Demo
(
	Id INT NOT NULL
)            
            ";
            command.ExecuteNonQuery();
            
            Console.WriteLine("create primary key PK_Demo");

            command.CommandText = 'CREATE TABLE dbo.Demo ( Id INT NOT NULL )';
            command.ExecuteNonQuery();
        }
    }
}
```