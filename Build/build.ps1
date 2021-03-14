#Requires -Modules @{ModuleName='InvokeBuild'; RequiredVersion='5.7.2'}

Set-StrictMode -Version Latest

$file = Join-Path $PSScriptRoot "build-tasks.ps1"
Invoke-Build -File $file