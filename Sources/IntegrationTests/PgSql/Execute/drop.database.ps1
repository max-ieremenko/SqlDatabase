[CmdletBinding()]
param (
    $Command,
    $Variables
)

$Command.Connection.ChangeDatabase("postgres");

Write-Information ("drop " + $Variables.DatabaseName)

$Command.CommandText = ("DROP DATABASE {0} WITH (FORCE)" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()

