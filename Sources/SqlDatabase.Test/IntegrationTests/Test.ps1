param (
    $bin,
    $connectionString
)

$ErrorActionPreference = "Stop"

$app = Join-Path $bin "SqlDatabase.dll"
$scripts = Join-Path $PSScriptRoot "New"

Write-Host "----- create new database ---"
$scripts = Join-Path $PSScriptRoot "New"
Exec {
    dotnet $app create `
        "-database=$connectionString" `
        "-from=$scripts" `
        -varJohnCity=London `
        -varMariaCity=Paris
}

Write-Host "----- update database ---"
$scripts = Join-Path $PSScriptRoot "Upgrade"
Exec {
    dotnet $app upgrade `
        "-database=$connectionString" `
        "-from=$scripts" `
        -varJohnSecondName=Smitt `
        -varMariaSecondName=X
}

Write-Host "----- update database (modularity) ---"
$scripts = Join-Path $PSScriptRoot "UpgradeModularity"
$configuration = (Join-Path $scripts "SqlDatabase.exe.config")
Exec {
    dotnet $app upgrade  `
        "-database=$connectionString"  `
        "-from=$scripts" `
        "-configuration=$configuration"
}

Write-Host "----- export data ---"
$scripts = Join-Path $PSScriptRoot "Export/export.sql"
Exec {
    dotnet $app export  `
        "-database=$connectionString"  `
        "-from=$scripts" `
        "-toTable=dbo.ExportedData1"
}

Write-Host "----- execute script ---"
$scripts = Join-Path $PSScriptRoot "execute/drop.database.sql"
Exec {
    dotnet $app execute  `
        "-database=$connectionString"  `
        "-from=$scripts"
}