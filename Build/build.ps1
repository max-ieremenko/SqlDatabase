#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.10.4" }

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$file = Join-Path $PSScriptRoot "build-tasks.ps1"
Invoke-Build -File $file