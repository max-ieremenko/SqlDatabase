function Wait-Mssql {
    param (
        [Parameter(Mandatory)]
        [string]
        $ConnectionString
    )
    
    Wait-Connection -ConnectionName System.Data.SqlClient.SqlConnection -ConnectionString $ConnectionString
}