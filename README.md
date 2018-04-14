# SqlDatabase
This is a code for a SQL Server database migration tool.

#### Requirements

Microsoft [.NET Framework 4.5.2](https://www.microsoft.com/en-us/download/details.aspx?id=42642) or higher.

#### Download

Download the [latest release](https://github.com/max-ieremenko/SqlDatabase/releases).

#### CLI
```bash
$ SqlDatabase.exe create
      "-database=Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True"
      -from=Examples\MigrationStepsFolder
      -varVariable1=value1
      -varVariable2=value2
```
create new database *MyDatabase* on Sql Server *MyServer* based on scripts from [Examples\CreateDatabaseFolder](Examples/CreateDatabaseFolder) with "Variable1=value1" and "Variable2=value2"


```bash
$ SqlDatabase.exe upgrade
      "-database=Data Source=server;Initial Catalog=MyDatabase;Integrated Security=True"
	  -from=Examples\MigrationStepsFolder
	  -varVariable1=value1
	  -varVariable2=value2
```
upgrade existing database *MyDatabase* on Sql Server *MyServer* based on scripts from [Examples\MigrationStepsFolder](Examples/MigrationStepsFolder) with "Variable1=value1" and "Variable2=value2"

```bash
$ SqlDatabase.exe execute
      "-database=Data Source=server;Initial Catalog=database;Integrated Security=True"
	  -from=c:\Scripts\script.sql
	  -varVariable1=value1
	  -varVariable2=value2
```
execute script from "c:\Scripts\script.sql" on "database" on "server" with "Variable1=value1" and "Variable2=value2"

|Switch|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|path to a folder or .zip file with scripts or file name for execute command|
|-transaction|set transaction mode (none, perStep). Option [none] is default, means no trasactions. Option [perStep] means to use one transaction per each migration step|
|[-var]|set a variable in format "=var[name of variable]=[value of variable]"|

Exit codes
* 0 - OK
* 1 - invalid command line
* 2 - errors during execution

#### Script types
- *.sql* a text file with Sql Server scripts
- *.dll* or *.exe* an .NET assembly with a script implementation, see [an example](Examples/CSharpMirationStep)

#### Variables in .sql scripts
Any entry like *{{VariableName}}* or *$(VariableName)* is interpreted as variable and has to be changed (text replacement) with active value before script execution. The name is case insensitive.
Non defined value of a variable leads to and error and stops migration execution.

The value is resolving in the following order:
1. check command line
2. check environment variable (Environment.GetEnvironmentVariable())

#### .zip files support
Parameter *-from* in the command line interprets .zip files in the path as folders, for example
*-from=c:\scripts.zip\archive\tables.zip\demo* or *-from=c:\scripts.zip\archive\tables.zip\table1.sql*

#### [Example](Examples/MigrationStepsFolder/) of a folder or .zip file with migration steps
|File|Description|
|:--|:----------|
|1.0_1.3.zip|.zip file with archived migration steps from database version 1.0 to 1.3 (inclusive).|
|1.3_2.0.sql|the database migration step from version 1.3 to 2.0|
|2.0_2.1.sql|the database migration step from version 2.0 to 2.1|
|2.1_2.2.dll|the database migration step from version 2.1 to 2.2|
|2.2_2.3.exe|the database migration step from version 2.2 to 2.3|
|2.3_3.0.sql|the database migration step from version 2.3 to 3.0|

#### [Example](Examples/CreateDatabaseFolder/) of a folder or .zip file with creation scripts
- 01_database
    - 01_DropExistsing.sql
    - 02_Create.sql
    - 03_Version.sql
- 02_schemas
    - 01_demo.sql
- 03_tables
    - 01_demo.Department.sql
    - 02_demo.Employee.sql


#### License
This tool is distributed under the [MIT](LICENSE) license.
