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

CLI
===

|Option|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|a path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.|
|-configuration|a path to application configuration file. Default is current [SqlDatabase.exe.config](../ConfigurationFile)|
|-log|optional path to log file|
|-var|set a variable in format "=var[name of variable]=[value of variable]"|
|-whatIf|shows what would happen if the command runs. The command is not run|

#### -from

```bash
# create a new database from script files in CreateScripts folder
-from=C:\MyDatabase\CreateScripts

# create a new database from script files in CreateScripts.zip archive
-from=C:\MyDatabase\CreateScripts.zip

# create a new database from script files in CreateScripts folder in MyDatabase.zip archive
-from=C:\MyDatabase.zip\CreateScripts

# create a new database from scripts in file CreateScript.sql
-from=C:\MyDatabase\CreateScript.sql

# create a new database from scripts in file CreateScript.sql in MyDatabase.zip archive
-from=C:\MyDatabase.zip\CreateScript.sql
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

File name 01_database/02_Create.sql

```sql
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

.ps1 script example
=============================

File name 01_database/02_Create.ps1, see details [here](../PowerShellScript).

```powershell
param (
    $Command,
    $Variables
)

Write-Information "start execution"

$Command.Connection.ChangeDatabase("master")

$Command.CommandText = ("CREATE DATABASE [{0}]" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()

$Command.CommandText = ("ALTER DATABASE [{0}] SET RECOVERY SIMPLE WITH NO_WAIT" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()

$Command.CommandText = ("ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION ON" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()

Write-Information "finish execution"
```

Assembly script example
=======================

File name 01_database/02_Create.dll, see details [here](../CSharpMirationStep).

```C#
namespace <any namespace name>
{
    public /*sealed*/ class SqlDatabaseScript /*: IDisposable*/
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            Console.WriteLine("start execution");

            command.Connection.ChangeDatabase("master");

            command.CommandText = string.Format("CREATE DATABASE [{0}]", variables["DatabaseName"]);
            command.ExecuteNonQuery();

            command.CommandText = string.Format("ALTER DATABASE [{0}] SET RECOVERY SIMPLE WITH NO_WAIT", variables["DatabaseName"]);
            command.ExecuteNonQuery();

            command.CommandText = string.Format("ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION ON", variables["DatabaseName"]);
            command.ExecuteNonQuery();

            Console.WriteLine("finish execution");
        }
    }
}
```
