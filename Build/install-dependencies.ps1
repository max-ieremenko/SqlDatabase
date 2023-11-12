#Requires -Version "7.0"

[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet(".net", "InvokeBuild", "ThirdPartyLibraries")] 
    [string[]]
    $List = (".net", "InvokeBuild", "ThirdPartyLibraries")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "scripts/Get-ModuleVersion.ps1")
. (Join-Path $PSScriptRoot "scripts/Invoke-InstallDotNet.ps1")
. (Join-Path $PSScriptRoot "scripts/Invoke-InstallModule.ps1")

if (".net" -in $List) {
    Invoke-InstallDotNet -Version "6.0.319"
    Invoke-InstallDotNet -Version "7.0.100"

    $version = (Get-Content -Raw (Join-Path $PSScriptRoot "../Sources/global.json") | ConvertFrom-Json).sdk.version
    Invoke-InstallDotNet -Version $version
}

if ("InvokeBuild" -in $List) {
    $version = Get-ModuleVersion "InvokeBuild"
    Invoke-InstallModule -Name "InvokeBuild" -Version $version
}

if ("ThirdPartyLibraries" -in $List) {
    $version = Get-ModuleVersion "ThirdPartyLibraries"
    Invoke-InstallModule -Name "ThirdPartyLibraries" -Version $version
}