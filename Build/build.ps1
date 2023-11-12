#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.10.4" }
#Requires -Modules @{ ModuleName="ThirdPartyLibraries"; ModuleVersion="3.4.1" }

[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("local", "github")] 
    [string]
    $Mode,

    [Parameter()]
    [string]
    $GithubToken
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$file = Join-Path $PSScriptRoot "tasks/build-tasks.ps1"
$task = ($Mode -eq "github") ? "GithubBuild" : "LocalBuild"

Invoke-Build -File $file -Task $task -GithubToken $GithubToken