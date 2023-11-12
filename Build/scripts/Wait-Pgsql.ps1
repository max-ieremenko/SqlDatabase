function Wait-Pgsql {
    param (
        [Parameter(Mandatory)]
        [string]
        $ConnectionString
    )
    
    Wait-Connection -ConnectionName Npgsql.NpgsqlConnection -ConnectionString $ConnectionString
}