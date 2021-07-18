SqlDatabase
===========

[![NuGet](https://img.shields.io/nuget/v/SqlDatabase.svg?style=flat-square&label=nuget%20net%204.5.2)](https://www.nuget.org/packages/SqlDatabase/)
[![NuGet](https://img.shields.io/nuget/v/SqlDatabase.GlobalTool.svg?style=flat-square&label=nuget%20dotnet%20tool)](https://www.nuget.org/packages/SqlDatabase.GlobalTool/)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/SqlDatabase.svg?style=flat-square)](https://www.powershellgallery.com/packages/SqlDatabase)
[![GitHub release](https://img.shields.io/github/release/max-ieremenko/SqlDatabase.svg?style=flat-square&label=manual%20download)](https://github.com/max-ieremenko/SqlDatabase/releases)

Command-line tool and PowerShell module for MSSQL Server, PostgreSQL and MySQL allows to execute scripts, database migrations and export data.

Table of Contents
-----------------

<!-- toc -->

- [Installation](#installation)
- [Target database type](#database-selection)
- [Execute script(s) (file)](#execute-script)
- [Export data from a database to sql script (file)](#export-data)
- [Create a database](#create-database)
- [Migrate an existing database](#upgrade-database)
- [Scripts](#scripts)
- [Variables](#variables)
- [*.zip files](#zip-files)
- [VS Package manager console](#console)
- [Examples](#examples)
- [License](#license)

<!-- tocstop -->

Installation
------------

PowerShell module is compatible with Powershell Core 6.1+ and PowerShell Desktop 5.1.

.net tool requires SDK .Net 5.0 or .Net Core 2.1/3.1.

Command-line tool is compatible with .net runtime 5.0, .net Core runtime 2.1/3.1 and .net Framework 4.5.2+.

### PowerShell, from gallery

[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/SqlDatabase.svg?style=flat-square)](https://www.powershellgallery.com/packages/SqlDatabase)

```powershell
PS> Install-Module -Name SqlDatabase
```

### PowerShell, manual [release](https://github.com/max-ieremenko/SqlDatabase/releases) download

[![GitHub release](https://img.shields.io/github/release/max-ieremenko/SqlDatabase.svg?style=flat-square&label=manual%20download)](https://github.com/max-ieremenko/SqlDatabase/releases)

```powershell
PS> Import-Module .\SqlDatabase.psm1
```

### Dotnet sdk tool

[![NuGet](https://img.shields.io/nuget/v/SqlDatabase.GlobalTool.svg?style=flat-square&label=nuget%20dotnet%20tool)](https://www.nuget.org/packages/SqlDatabase.GlobalTool/)

```bash
$ dotnet tool install --global SqlDatabase.GlobalTool
```

[Back to ToC](#table-of-contents)

Target database type selection <a name="database-selection"></a>
--------------

The target database/server type is recognized automatically from provided connection string:

here is target MSSQL Server (keywords `Data Source` and `Initial Catalog`):

```bash
$ SqlDatabase [command] "-database=Data Source=server;Initial Catalog=database;Integrated Security=True"

PS> *-SqlDatabase -database "Data Source=server;Initial Catalog=database;Integrated Security=True"
```

here is target PostgreSQL (keywords `Host` and `Database`):

```bash
$ SqlDatabase [command] "-database=Host=server;Username=postgres;Password=qwerty;Database=database"

PS> *-SqlDatabase -database "Host=server;Username=postgres;Password=qwerty;Database=database"
```

here is target MySQL (keywords `Server` and `Database`):

```bash
$ SqlDatabase [command] "-database=Server=localhost;Database=database;User ID=root;Password=qwerty;"

PS> *-SqlDatabase -database "Server=localhost;Database=database;User ID=root;Password=qwerty;"
```

[Back to ToC](#table-of-contents)

Execute script(s) <a name="execute-script"></a>
--------------

execute script from file "c:\Scripts\script.sql" on *[MyDatabase]* on server *[MyServer]* with "Variable1=value1" and "Variable2=value2"

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

 See more details [here](Examples/ExecuteScriptsFolder).

[Back to ToC](#table-of-contents)

Export data from a database to sql script (file) <a name="export-data"></a>
--------------

export data from sys.databases view into "c:\databases.sql" from "MyDatabase" on "server"

```bash
$ SqlDatabase export ^
      "-database=Data Source=server;Initial Catalog=database;Integrated Security=True" ^
      "-fromSql=SELECT * FROM sys.databases" ^
      -toFile=c:\databases.sql

PS> Export-SqlDatabase `
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True" `
      -fromSql "SELECT * FROM sys.databases" `
      -toFile c:\databases.sql `
      -InformationAction Continue
```

 See more details [here](Examples/ExportData).

[Back to ToC](#table-of-contents)

Create a database <a name="create-database"></a>
---------------

create new database *[MyDatabase]* on server *[MyServer]* from scripts in *[Examples\CreateDatabaseFolder]* with "Variable1=value1" and "Variable2=value2"

```bash
$ SqlDatabase create ^
      "-database=Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True" ^
      -from=Examples\CreateDatabaseFolder ^
      -varVariable1=value1 ^
      -varVariable2=value2

PS> Create-SqlDatabase `
      -database "Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True" `
      -from Examples\CreateDatabaseFolder `
      -var Variable1=value1,Variable2=value2 `
      -InformationAction Continue
```

 See more details [here](Examples/CreateDatabaseFolder).

[Back to ToC](#table-of-contents)

Migrate an existing database <a name="upgrade-database"></a>
----------------

upgrade existing database *[MyDatabase]* on server *[MyServer]* from scripts in *Examples\MigrationStepsFolder* with "Variable1=value1" and "Variable2=value2"

```bash
$ SqlDatabase upgrade ^
      "-database=Data Source=server;Initial Catalog=MyDatabase;Integrated Security=True" ^
      -from=Examples\MigrationStepsFolder ^
      -varVariable1=value1 ^
      -varVariable2=value2

PS> Upgrade-SqlDatabase `
      -database "Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True" `
      -from Examples\MigrationStepsFolder `
      -var Variable1=value1,Variable2=value2 `
      -InformationAction Continue
```

 See more details [here](Examples/MigrationStepsFolder).

[Back to ToC](#table-of-contents)

Scripts
-------

- *.sql* a text file with sql scripts
- *.ps1* a text file with PowerShell script, details are [here](Examples/PowerShellScript)
- *.dll* or *.exe* an .NET assembly with a script implementation, details are [here](Examples/CSharpMirationStep)

[Back to ToC](#table-of-contents)

Variables
---------

In a sql text file any entry like *{{VariableName}}* or *$(VariableName)* is interpreted as variable and has to be changed (text replacement) with a value before script execution.
The variable name is

- a word from characters a-z, A-Z, 0-9, including the _ (underscore) character
- case insensitive

#### Example

```sql
-- script.sql
PRINT 'drop table {{Schema}}.{{Table}}'
DROP TABLE [{{Schema}}].[{{Table}}]
```

```bash
# execute script.sql
$ SqlDatabase execute -from=script.sql -varSchema=dbo -varTable=Person
PS> Execute-SqlDatabase -from script.sql -var Schema=dbo,Table=Person -InformationAction Continue

# log output
script.sql ...
   variable Schema was replaced with dbo
   variable Table was replaced with Person
```

```sql
-- script at runtime
PRINT 'drop table dbo.Person'
DROP TABLE [dbo].[Person]
```

#### Example how to hide variable value from a log output

If a name of variable starts with _ (underscore) character, for instance *_Password*, the value of variable will not be shown in the log output.

```sql
-- script.sql
ALTER LOGIN [sa] WITH PASSWORD=N'{{_Password}}'
```

```bash
# execute script.sql
$ SqlDatabase execute -from=script.sql -var_Password=P@ssw0rd
PS> Execute-SqlDatabase -from script.sql -var _Password=P@ssw0rd -InformationAction Continue

# log output
script.sql ...
   variable _Password was replaced with [value is hidden]
```

```sql
-- script at runtime
ALTER LOGIN [sa] WITH PASSWORD=N'{{P@ssw0rd}}'
```

A non defined variable`s value leads to an error and stops script execution process.

The variable value is resolved in the following order:

1. check command line
2. check environment variables (Environment.GetEnvironmentVariable())
3. check [configuration file](Examples/ConfigurationFile)

### Predefined variables

- *DatabaseName* - the target database name, see connection string (-database=...Initial Catalog=MyDatabase...)
- *CurrentVersion* - the database/module version before execution of a [migration step](Examples/MigrationStepsFolder)
- *TargetVersion* - the database/module version after execution of a [migration step](Examples/MigrationStepsFolder)
- *ModuleName* - the module name of current [migration step](Examples/MigrationStepsFolder), empty string in case of straight forward upgrade

[Back to ToC](#table-of-contents)

*.zip files <a name="zip-files"></a>
------------------------------------

Parameters *-from* and *-configuration* in the command line interpret .zip files in the path as folders, for example

- -from=c:\scripts.zip\archive\tables.zip\demo
- -from=c:\scripts.zip\archive\tables.zip\table1.sql
- -configuration=c:\scripts.zip\app.config

[Back to ToC](#table-of-contents)


VS Package manager console <a name="console"></a>
------------------------------------------------

For integrating SqlDatabase into the Visual studio package manager console please check this [example](Examples/PackageManagerConsole).

[Back to ToC](#table-of-contents)

Examples
--------

- [create ms sql server linux docker image](Examples/SqlServerDockerImage)
- [execute script(s)](Examples/ExecuteScriptsFolder)
- [export data](Examples/ExportData)
- [create a database](Examples/CreateDatabaseFolder)
- [upgrade an existing database](Examples/MigrationStepsFolder)
- [how to use SqlDatabase in the VS Package manager console](Examples/PackageManagerConsole)
- [configuration file](Examples/ConfigurationFile)
- [assembly script](Examples/CSharpMirationStep)

[Back to ToC](#table-of-contents)

License
-------

This tool is distributed under the [MIT](LICENSE.md) license.

[Back to ToC](#table-of-contents)