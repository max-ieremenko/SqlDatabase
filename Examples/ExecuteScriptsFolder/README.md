Execute script(s) (file)
=================

```bash
$ SqlDatabase execute ^
      "-database=Data Source=server;Initial Catalog=database;Integrated Security=True" ^
      -from=c:\Scripts\script.sql ^
      -varVariable1=value1 ^
      -varVariable2=value2

PS> Execute-SqlDatabase `
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True" `
      -from c:\Scripts\script.sql `
      -var Variable1=value1,Variable2=value2 `
      -InformationAction Continue
```

execute script from file "c:\Scripts\script.sql" on *[MyDatabase]* on server *[MyServer]* with "Variable1=value1" and "Variable2=value2"

CLI
===

|Option|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|a path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.|
|-fromSql|an sql script text. Repeat -fromSql to setup several scripts.|
|-configuration|a path to application configuration file. Default is current [SqlDatabase.exe.config](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/ConfigurationFile)|
|[-var]|set a variable in format "=var[name of variable]=[value of variable]"|

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


Sql script example
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

Assembly script example
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