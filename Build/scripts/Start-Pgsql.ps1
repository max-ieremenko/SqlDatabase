function Start-Pgsql {
    param ()
    
    # https://www.nuget.org/packages/Npgsql/4.0.16 lib/netstandard2.0/Npgsql.dll
    $npgsqldll = Join-Path $PSScriptRoot 'Npgsql.dll'
    Add-Type -Path $npgsqldll

    $container = Start-Container -Image sqldatabase/postgres:18.0 -ContainerPort 5432

    $builder = New-Object -TypeName Npgsql.NpgsqlConnectionStringBuilder
    $builder['Database'] = 'sqldatabasetest'
    $builder['Username'] = 'adminuser'
    $builder['Password'] = 'qwerty'
    $builder['Timeout'] = 5

    $builder.Host = 'localhost'
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