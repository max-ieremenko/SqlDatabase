[CmdletBinding(SupportsShouldProcess=$true)]
param (
    $Command,
    $Variables
)

$Command.CommandText = "ALTER TABLE person ADD second_name varchar(250) NULL"
$Command.ExecuteNonQuery()
