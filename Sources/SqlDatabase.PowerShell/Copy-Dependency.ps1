#Requires -Version "7.0"

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $Destination,

    [Parameter(Mandatory)]
    [string[]]
    $AssemblyName,

    [string]
    $SourceTarget = '.NETStandard,Version=v2.0',

    [string]
    $Source
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($Source) {
    $assetsFile = Join-Path $PSScriptRoot $Source 'obj' 'project.assets.json'
}
else {
    $assetsFile = Join-Path $PSScriptRoot 'obj' 'project.assets.json'
}

$assets = Get-Content -Path $assetsFile -Raw | ConvertFrom-Json
$targets = $assets.targets.$SourceTarget

$nugetCache = Join-Path $HOME .nuget/packages
$copyTo = Join-Path $PSScriptRoot $Destination

foreach ($name in $AssemblyName) {
    $member = $targets | Get-Member -MemberType Properties | Where-Object { $_.Name -like "$name/*" }
    if (-not $member) {
        throw "Dependency $name not found"
    }
        
    $packagePath = $member.Name.ToLowerInvariant() # Microsoft.Bcl.AsyncInterfaces/8.0.0
    $filePath = ($targets."$($member.Name)".runtime | Get-Member -MemberType Properties).Name #lib/netstandard2.0/Microsoft.Bcl.AsyncInterfaces.dll
    $file = Join-Path $nugetCache $packagePath $filePath

    Write-Host "copy $file to $copyTo"
    Copy-Item -Path $file -Destination $copyTo
}