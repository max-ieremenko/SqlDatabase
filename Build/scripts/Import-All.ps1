Get-ChildItem -Path $PSScriptRoot -Filter *.ps1 | Where-Object { $_.Name -ne 'Import-All.ps1' } | ForEach-Object { . $_.FullName }

$currentEnv = Get-CurentEnvironment
Get-ChildItem -Path (Join-Path $PSScriptRoot $currentEnv) -Filter *.ps1 | ForEach-Object { . $_.FullName }