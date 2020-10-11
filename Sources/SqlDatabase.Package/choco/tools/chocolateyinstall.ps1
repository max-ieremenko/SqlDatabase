$ErrorActionPreference = "Stop"
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

if (Get-Module "SqlDatabase") {
	Remove-Module "SqlDatabase" -Force
}

$psd1File = Join-Path $toolsDir "SqlDatabase\SqlDatabase.psd1"
$psd1 = Test-ModuleManifest -Path $psd1File -WarningAction Ignore -ErrorAction Stop

$destination = Join-Path $env:ProgramFiles ("WindowsPowerShell\Modules\SqlDatabase\" + $psd1.Version.ToString())
if (Test-Path $destination) {
  Remove-Item -Path $destination -Force -Recurse
}

$source = Join-Path $toolsDir "SqlDatabase\*"
New-Item -Path $destination -ItemType Directory | Out-Null
Copy-Item -Path $source -Destination $destination
