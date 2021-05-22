$ErrorActionPreference = "Stop"

$connectionString = $env:connectionString

Import-Module "SqlDatabase"

Write-Host "----- create new database ---"
Create-SqlDatabase `
    -database $connectionString `
    -from "New" `
    -var JohnCity=London,MariaCity=Paris

Write-Host "----- update database ---"
Upgrade-SqlDatabase `
    -database $connectionString `
    -from "Upgrade" `
    -var JohnSecondName=Smitt,MariaSecondName=X `
    -configuration "Upgrade/SqlDatabase.exe.config"

Write-Host "----- update database (modularity) ---"
Upgrade-SqlDatabase `
    -database $connectionString `
    -from "UpgradeModularity" `
    -configuration "UpgradeModularity/SqlDatabase.exe.config"

Write-Host "----- export data ---"
Export-SqlDatabase `
    -database $connectionString `
    -from "Export/export.sql" `
    -toTable "public.sqldatabase_export1"

Write-Host "----- execute script ---"
Execute-SqlDatabase `
    -database $connectionString `
    -from "execute/drop.database.ps1"
