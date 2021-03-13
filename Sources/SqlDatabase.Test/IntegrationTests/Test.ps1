param (
    $app,
    $connectionString
)

$ErrorActionPreference = "Stop"

Write-Host "----- create new database ---"
$scripts = Join-Path $PSScriptRoot "New"
exec {
    & $app create `
        "-database=$connectionString" `
        "-from=$scripts" `
        -varJohnCity=London `
        -varMariaCity=Paris
}

Write-Host "----- update database ---"
$scripts = Join-Path $PSScriptRoot "Upgrade"
exec {
    & $app upgrade `
        "-database=$connectionString" `
        "-from=$scripts" `
        -varJohnSecondName=Smitt `
        -varMariaSecondName=X
}

Write-Host "----- update database (modularity) ---"
$scripts = Join-Path $PSScriptRoot "UpgradeModularity"
$configuration = (Join-Path $scripts "SqlDatabase.exe.config")
exec {
    & $app upgrade  `
        "-database=$connectionString"  `
        "-from=$scripts" `
        "-configuration=$configuration"
}

Write-Host "----- export data ---"
$scripts = Join-Path $PSScriptRoot "Export/export.sql"
exec {
    & $app export  `
        "-database=$connectionString"  `
        "-from=$scripts" `
        "-toTable=dbo.ExportedData1"
}

Write-Host "----- execute script ---"
$scripts = Join-Path $PSScriptRoot "execute/drop.database.sql"
exec {
    & $app execute  `
        "-database=$connectionString"  `
        "-from=$scripts"
}