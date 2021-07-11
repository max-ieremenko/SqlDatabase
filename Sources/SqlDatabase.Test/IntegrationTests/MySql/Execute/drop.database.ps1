[CmdletBinding()]
param (
    $Command,
    $Variables
)

Write-Information ("drop " + $Variables.DatabaseName)

$Command.CommandText = ("DROP DATABASE {0}" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()

