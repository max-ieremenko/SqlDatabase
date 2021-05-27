.ps1 script
==========================================

SqlDatabase supports powershell scripts for commands [execute](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/ExecuteScriptsFolder), [create](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/CreateDatabaseFolder) and [upgrade](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/MigrationStepsFolder).

Example:

```bash
$ SqlDatabase execute ^
      "-database=Data Source=server;Initial Catalog=database;Integrated Security=True" ^
      -from=c:\script.ps1

PS> Execute-SqlDatabase `
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True" `
      -from c:\script.ps1 `
      -InformationAction Continue
```

script.ps1:

```powershell
[CmdletBinding(SupportsShouldProcess=$true)] # indicates that the script implementation supports -WhatIf scenario
param (
    $Command, # instance of SqlCommand, $null in case -WhatIf
    $Variables # access to variables
)

if (-not $Variables.TableName) {
    throw "Variable TableName is not defined."
}

if ($WhatIfPreference) {
    # handle -WhatIf scenario
    return
}

Write-Information "start execution"

$Command.CommandText = ("print 'current database name is {0}'" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()

$Command.CommandText = ("drop table {0}" -f $Variables.TableName)
$Command.ExecuteNonQuery()

Write-Information "finish execution"
```

use

* cmdlet parameter binding
* parameter `$Command` to affect database
* parameter `$Variables` to access variables
* `Write-*` to write something into output/log
* `SupportsShouldProcess=$true` and `$WhatIfPreference` if script supports `-WhatIf` scenario

## Which version of PowerShell is used to run .ps1

### SqlDatabase powershell module

[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/SqlDatabase.svg?style=flat-square)](https://www.powershellgallery.com/packages/SqlDatabase)

The version with which you run the module.

### .net framework 4.5.2

[![NuGet](https://img.shields.io/nuget/v/SqlDatabase.svg?style=flat-square&label=nuget%20net%204.5.2)](https://www.nuget.org/packages/SqlDatabase/)

Installed Powershell Desktop version.

### .net SDK tool for .net 5.0 or .net core 2.1/3.1

[![NuGet](https://img.shields.io/nuget/v/SqlDatabase.GlobalTool.svg?style=flat-square&label=nuget%20dotnet%20tool)](https://www.nuget.org/packages/SqlDatabase.GlobalTool/)

Pre-installed Powershell Core is required, will be used by SqlDatabase as external component. Due to Powershell Core design,

* SqlDatabase .net 5.0 can host Powershell Core versions below 7.2
* .net core 3.1 below 7.1
* .net core 2.1 below 7.0

PowerShell location can be passed via command line:

```bash
$ SqlDatabase execute ^
      -usePowerShell=C:\Program Files\PowerShell\7

$ dotnet SqlDatabase.dll create ^
      -usePowerShell=/opt/microsoft/powershell/7
```

PowerShell location by default:

* if SqlDatabase is running by PowerShell (parent process is PowerShell) and version is compatible, use this version
* check well-known installation folders: `C:\Program Files\PowerShell` on windows and `/opt/microsoft/powershell` on linux, use latest compatible version

## Scripts isolation

For each .ps1 script, executing by SqlDatabase

* create new PowerShell session with default cmdlets, providers, built-in functions, aliases etc.
* run script as `script block`
* destroy the session
