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
|-configuration|a path to application configuration file. Default is current [SqlDatabase.exe.config](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/ConfigurationFile)|
|-var|set a variable in format "=var[name of variable]=[value of variable]"|

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

#### Example
The following script is used by SqlDatabase to resolve the current database version, details are in [configuration file](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/ConfigurationFile)

```sql
-- select current version
SELECT value from sys.fn_listextendedproperty('version', default, default, default, default, default, default)

-- output
-- 1.2
```

The current version is *1.2*, so we have the following migration steps sequence:
1. 1.0_1.3.zip\1.2_1.3.sql
2. 1.3_2.0.sql
3. 2.0_2.1.sql
4. 2.1_2.2.sql
5. 2.2_3.0.sql
6. 3.0_3.1.sql
7. 3.0_4.0.sql

Each step will be executed one by one:
```sql
/* 1.0_1.3.zip\1.2_1.3.sql */
execute 1.0_1.3.zip\1.2_1.3.sql
-- update current version
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'1.3'

/* 1.3_2.0.sql */
execute 1.3_2.0.sql
-- update current version
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'2.0'

-- ....
```

Predefined variables
====================

|Name|Description|
|:--|:----------|
|DatabaseName|the target database name|
|CurrentVersion|the database version before execution of a migration step|
|TargetVersion|the database version after execution of a migration step|


Migration .sql step example
=============================
```sql
-- 2.0_2.1.sql
PRINT 'create table Demo'
GO

CREATE TABLE dbo.Demo
(
	Id INT NOT NULL
)
GO

ALTER TABLE dbo.Demo ADD CONSTRAINT PK_Demo PRIMARY KEY CLUSTERED (Id)
GO
```

Migration .dll step example
=======================

```C#
namespace <any namespace name>
{
    public /*sealed*/ class SqlDatabaseScript /*: IDisposable*/
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            // write a message to an migration log
            Console.WriteLine("start execution");

            // execute a query
            command.CommandText = "create table Demo");
            command.ExecuteNonQuery();

            // execute a query
            command.CommandText = 'CREATE TABLE dbo.Demo ( Id INT NOT NULL )';
            command.ExecuteNonQuery();

            // execute a query
            command.CommandText = 'ALTER TABLE dbo.Demo ADD CONSTRAINT PK_Demo PRIMARY KEY CLUSTERED (Id)';
            command.ExecuteNonQuery();

            // write a message to a log
            Console.WriteLine("finish execution");
        }
    }
}
```