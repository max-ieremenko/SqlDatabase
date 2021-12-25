#Requires -Modules @{ModuleName='InvokeBuild'; RequiredVersion='5.8.7'}

Set-StrictMode -Version Latest

$file = Join-Path $PSScriptRoot "create-images-tasks.ps1"
Invoke-Build -File $file