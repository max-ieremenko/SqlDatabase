#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.11.0" }

Set-StrictMode -Version Latest

$file = Join-Path $PSScriptRoot "tasks/create-images-tasks.ps1"
Invoke-Build -File $file