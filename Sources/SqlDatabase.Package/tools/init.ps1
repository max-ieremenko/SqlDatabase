param($installPath, $toolsPath, $package, $project)

$obsoleteModule = Get-Module "SqlDatabase.PowerShell"
if ($obsoleteModule)
{
	Remove-Module "SqlDatabase.PowerShell"
}

$activeModule = Get-Module "SqlDatabase"
$thisModule = Join-Path $PSScriptRoot "SqlDatabase.psd1"

$import = $true
if ($activeModule)
{
	$thisModuleFile = Join-Path $PSScriptRoot "SqlDatabase.PowerShell.dll"
	$thisModuleVersion = New-Object -TypeName System.Version (Get-Item $thisModuleFile).VersionInfo.FileVersion
    if ($activeModule.Version -le $thisModuleVersion)
    {
        Remove-Module "SqlDatabase"
    }
    else
    {
        $import = $false
    }
}

if ($import)
{
    Import-Module $thisModule
}
