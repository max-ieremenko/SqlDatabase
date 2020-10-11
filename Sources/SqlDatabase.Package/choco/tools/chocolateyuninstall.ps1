$ErrorActionPreference = "Stop"

if (Get-Module "SqlDatabase") {
	Remove-Module "SqlDatabase" -Force
}

$source = Join-Path -Path $env:ProgramFiles -ChildPath "WindowsPowerShell\Modules\SqlDatabase"
Remove-Item -Path $source -Recurse -Force
