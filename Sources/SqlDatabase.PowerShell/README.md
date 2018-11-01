## Installation

```bash
PS> Import-Module .\SqlDatabase.PowerShell.dll -DisableNameChecking
```

## Create database
```bash
PS> Create-SqlDatabase
      -database "Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True"
      -from Examples\CreateDatabaseFolder
      -var Variable1=value1,Variable2=value2
```

## Upgrade database
```bash
PS> Update-SqlDatabase
      -database "Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True"
      -from Examples\MigrationStepsFolder
      -var Variable1=value1,Variable2=value2
```

## Execute script
```bash
PS> Execute-SqlDatabase
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True"
      -from c:\Scripts\script.sql
      -var Variable1=value1,Variable2=value2

PS> Get-ChildItem c:\Scripts\ -Filter *.sql
    | Invoke-SqlDatabase
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True"
      -var Variable1=value1,Variable2=value2
```
