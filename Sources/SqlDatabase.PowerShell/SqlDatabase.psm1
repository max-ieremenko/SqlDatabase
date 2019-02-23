Set-StrictMode -Version Latest

$psModule = $ExecutionContext.SessionState.Module
$root = $psModule.ModuleBase
$dllPath = Join-Path -Path $root "SqlDatabase.PowerShell.dll"

$importedModule = Import-Module -Name $dllPath -PassThru

# When the module is unloaded, remove the nested binary module that was loaded with it
$psModule.OnRemove = {
    Remove-Module -ModuleInfo $importedModule
}
