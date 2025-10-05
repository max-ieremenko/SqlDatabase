#Requires -Version "7.0"
#Requires -Modules @{ ModuleName="InvokeBuild"; ModuleVersion="5.14.19" }
#Requires -Modules @{ ModuleName="ThirdPartyLibraries"; ModuleVersion="3.8.0" }

[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet('local', 'github')] 
    [string]
    $Mode,

    [Parameter()]
    [string]
    $GithubToken
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$file = Join-Path $PSScriptRoot 'tasks/build-tasks.ps1'
$task = ($Mode -eq 'github') ? 'GithubBuild' : 'LocalBuild'

Invoke-Build -File $file -Task $task -GithubToken $GithubToken