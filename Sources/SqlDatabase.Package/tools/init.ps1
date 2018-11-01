param($installPath, $toolsPath, $package, $project)

$activeModule = Get-Module "SqlDatabase.PowerShell"
$thisModule = Join-Path $PSScriptRoot "SqlDatabase.PowerShell.dll"

$import = $true
if ($activeModule)
{
	$thisModuleVersion = New-Object -TypeName System.Version (Get-Item $thisModule).VersionInfo.FileVersion
    if ($activeModule.Version -le $thisModuleVersion)
    {
        Remove-Module "SqlDatabase.PowerShell"
    }
    else
    {
        $import = $false
    }
}

if ($import)
{
    Import-Module $thisModule -DisableNameChecking
}
