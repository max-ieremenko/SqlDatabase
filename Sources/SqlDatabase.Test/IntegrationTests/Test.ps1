$ErrorActionPreference = "Stop"

$connectionString = $env:connectionString
$test = $env:test

Import-Module "SqlDatabase"

Write-Host "----- create new database ---"
Create-SqlDatabase `
    -database $connectionString `
    -from (Join-Path $test "New") `
    -var JohnCity=London,MariaCity=Paris

Write-Host "----- update database ---"
Upgrade-SqlDatabase `
    -database $connectionString `
    -from (Join-Path $test "Upgrade") `
    -var JohnSecondName=Smitt,MariaSecondName=X 

Write-Host "----- update database (modularity) ---"
Upgrade-SqlDatabase `
    -database $connectionString `
    -from (Join-Path $test "UpgradeModularity") `
    -configuration (Join-Path $test "UpgradeModularity/SqlDatabase.exe.config")

Write-Host "----- export data ---"
Export-SqlDatabase `
    -database $connectionString `
    -from (Join-Path $test "Export/export.sql") `
    -toTable "dbo.ExportedData1"

Write-Host "----- execute script ---"
Execute-SqlDatabase `
    -database $connectionString `
    -from (Join-Path $test "execute/drop.database.sql") 
