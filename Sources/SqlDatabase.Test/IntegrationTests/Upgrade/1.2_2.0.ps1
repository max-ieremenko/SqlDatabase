[CmdletBinding(SupportsShouldProcess=$true)]
param (
    $Command,
    $Variables
)

$Command.CommandText = "ALTER TABLE demo.Person ADD SecondName NVARCHAR(250) NULL"
$Command.ExecuteNonQuery()
