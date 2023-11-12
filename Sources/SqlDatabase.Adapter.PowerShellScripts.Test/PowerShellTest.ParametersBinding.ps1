[CmdletBinding(SupportsShouldProcess=$true)]
param (
    $Command,
    $Variables
)

if ($WhatIfPreference) {
    throw "not supported"
}

$Command.CommandText = ("database name is {0}" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()
