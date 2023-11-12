function Start-Pgsql {
    param ()
    
    $npgsqldll = Join-Path $env:USERPROFILE ".nuget\packages\npgsql\4.0.11\lib\netstandard2.0\Npgsql.dll"
    Add-Type -Path $npgsqldll

    $container = Start-Container -Image sqldatabase/postgres:13.3 -ContainerPort 5432

    $builder = New-Object -TypeName Npgsql.NpgsqlConnectionStringBuilder
    $builder["Database"] = "sqldatabasetest"
    $builder["Username"] = "postgres"
    $builder["Password"] = "qwerty"
    $builder["Timeout"] = 5

    $builder.Host = "localhost"
    $builder.Port = $container.port.ToString()
    $connectionString = $builder.ToString()
    
    $builder.Host = $container.ip.ToString()
    $builder.Port = 5432
    $remoteConnectionString = $builder.ToString()

    return @{
        containerId            = $container.containerId
        connectionString       = $connectionString
        remoteConnectionString = $remoteConnectionString
    }
}