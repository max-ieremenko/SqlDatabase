Set-StrictMode -Version Latest

if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -ne 'Desktop') -and ([version]$PSVersionTable.PSVersion -lt "6.1.0")) {
    Write-Error "This module requires PowerShell 6.1.0+. Please, upgrade your PowerShell version."
    Exit 1
}

$psModule = $ExecutionContext.SessionState.Module
$root = $psModule.ModuleBase
$dllPath = Join-Path -Path $root "SqlDatabase.PowerShell.dll"

$importedModule = Import-Module -Name $dllPath -PassThru

# When the module is unloaded, remove the nested binary module that was loaded with it
$psModule.OnRemove = {
    Remove-Module -ModuleInfo $importedModule
}
