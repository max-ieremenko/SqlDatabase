function Wait-Pgsql {
    param (
        [Parameter(Mandatory)]
        [string]
        $ConnectionString
    )
    
    if (-not ('Npgsql.NpgsqlConnection' -as [type])) {
        $npgsqldll = Join-Path $env:USERPROFILE '.nuget\packages\npgsql\4.0.11\lib\netstandard2.0\Npgsql.dll'
        Add-Type -Path $npgsqldll
    }

    Wait-Connection -ConnectionName Npgsql.NpgsqlConnection -ConnectionString $ConnectionString
}