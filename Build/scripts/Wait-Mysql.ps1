function Wait-Mysql {
    param (
        [Parameter(Mandatory)]
        [string]
        $ConnectionString
    )

    if (-not ('MySqlConnector.MySqlConnection' -as [type])) {
        $sqlConnectordll = Join-Path $env:USERPROFILE '\.nuget\packages\mysqlconnector\1.3.10\lib\netstandard2.0\MySqlConnector.dll'
        Add-Type -Path $sqlConnectordll
    }

    Wait-Connection -ConnectionName MySqlConnector.MySqlConnection -ConnectionString $ConnectionString
}