[CmdletBinding(SupportsShouldProcess=$true)]
param (
    $Command,
    $Variables
)

if ($WhatIfPreference) {
    Write-Host "WhatIf accepted"
} else {
    throw "not supported"
}