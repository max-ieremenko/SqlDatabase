[CmdletBinding(SupportsShouldProcess=$true)] # indicates that the script implementation supports -WhatIf scenario
param (
    $Command, # instance of SqlCommand, $null in case -WhatIf
    $Variables # access to variables
)

if (-not $Variables.TableName) {
    throw "Variable TableName is not defined."
}

if ($WhatIfPreference) {
    # handle -WhatIf scenario
    return
}

Write-Information "start execution"

$Command.CommandText = ("print 'current database name is {0}'" -f $Variables.DatabaseName)
$Command.ExecuteNonQuery()

$Command.CommandText = ("drop table {0}" -f $Variables.TableName)
$Command.ExecuteNonQuery()

Write-Information "finish execution"