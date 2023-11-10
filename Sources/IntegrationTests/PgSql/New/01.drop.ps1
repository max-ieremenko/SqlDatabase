[CmdletBinding()]
param (
    $Command,
    $Variables
)

$Command.Connection.ChangeDatabase("postgres");

$Command.CommandText = ("DROP DATABASE IF EXISTS {0} WITH (FORCE)" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()