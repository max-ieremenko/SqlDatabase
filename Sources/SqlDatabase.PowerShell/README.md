## Installation

```bash
$ Import-Module .\SqlDatabase.PowerShell.dll
```

## Execute script
```bash
PS> Invoke-SqlDatabase
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True"
      -from c:\Scripts\script.sql
      -var Variable1=value1,Variable2=value2

PS> Get-ChildItem c:\Scripts\ -Filter *.sql
    | Invoke-SqlDatabase
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True"
      -var Variable1=value1,Variable2=value2
```
