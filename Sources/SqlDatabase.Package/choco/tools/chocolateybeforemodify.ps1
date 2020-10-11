$ErrorActionPreference = "Stop"

if (Get-Module "SqlDatabase") {
	Remove-Module "SqlDatabase" -Force
}
