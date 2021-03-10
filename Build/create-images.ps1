#Install-Module -Name psake
#Requires -Modules @{ModuleName='psake'; RequiredVersion='4.9.0'}

$psakeMain = Join-Path $PSScriptRoot "create-images-tasks.ps1"
Invoke-psake $psakeMain