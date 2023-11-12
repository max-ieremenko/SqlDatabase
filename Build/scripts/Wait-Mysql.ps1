function Wait-Mysql {
    param (
        [Parameter(Mandatory)]
        [string]
        $ConnectionString
    )

    Wait-Connection -ConnectionName MySqlConnector.MySqlConnection -ConnectionString $ConnectionString
}