[CmdletBinding(SupportsShouldProcess=$true)]
param (
    $Command,
    $Variables
)

$Command.CommandText = "CREATE SCHEMA demo"
$Command.ExecuteNonQuery()
