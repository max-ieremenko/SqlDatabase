#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.9.12" }

Set-StrictMode -Version Latest

$file = Join-Path $PSScriptRoot "create-images-tasks.ps1"
Invoke-Build -File $file