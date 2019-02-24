Create a database
=================

```bash
$ SqlDatabase create ^
      "-database=Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True" ^
      -from=Examples\CreateDatabaseFolder ^
      -varVariable1=value1 ^
      -varVariable2=value2

PS> Create-SqlDatabase `
      -database "Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True" `
      -from Examples\CreateDatabaseFolder `
      -var Variable1=value1,Variable2=value2
```

create new database *MyDatabase* on Sql Server *MyServer* based on scripts from *Examples\CreateDatabaseFolder* with "Variable1=value1" and "Variable2=value2"

|Switch|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|path to a folder or .zip file with scripts. Repeat to setup several sources.|
|-configuration|path to application configuration file. Default is current [SqlDatabase.exe.config](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/ConfigurationFile)|
|[-var]|set a variable in format "=var[name of variable]=[value of variable]"|

Script`s execution order
========================

1. run all script`s files in the root folder, sorted alphabetically
2. run all script`s in each sub-folder, sorted alphabetically

|File|Execution order|
|:--|:----------|
|01_database|1|
|├── 01_DropExisting.sql|1.1|
|├── 02_Create.sql|1.2|
|└── 03_Version.sql|1.3|
|02_schemas|2|
|└── 01_demo.sql|2.1|
|03_tables|3|
|├── 01_demo.Department.sql|3.1|
|└── 02_demo.Employee.sql|3.2|
|05_data|4|
|└── 01_staff.sql|4.1|


Predefined variables
========================

|Name|Description|
|:--|:----------|
|DatabaseName|the target database name|


Sql script example
==================

```sql
-- 01_database/02_Create.sql
USE master
GO

CREATE DATABASE [{{DatabaseName}}]
GO

ALTER DATABASE [{{DatabaseName}}] SET RECOVERY SIMPLE WITH NO_WAIT
GO

ALTER DATABASE [{{DatabaseName}}] SET ALLOW_SNAPSHOT_ISOLATION ON
GO
```

```sql
-- at runtime
USE master
GO

CREATE DATABASE [MyDatabase]
GO

ALTER DATABASE [MyDatabase] SET RECOVERY SIMPLE WITH NO_WAIT
GO

ALTER DATABASE [MyDatabase] SET ALLOW_SNAPSHOT_ISOLATION ON
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
            command.CommandText = string.Format("print 'current database name is {0}'", variables["DatabaseName"]);
            command.ExecuteNonQuery();

            // execute a query
            command.CommandText = 'CREATE SCHEMA [demo]';
            command.ExecuteNonQuery();

            // write a message to a log
            Console.WriteLine("finish execution");
        }
    }
}
```
more details are [here](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/CSharpMirationStep)

