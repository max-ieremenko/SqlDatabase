function Start-Pgsql {
    param ()
    
    $container = Start-Container -Image sqldatabase/postgres:13.3 -ContainerPort 5432

    $builder = New-Object -TypeName System.Data.Common.DbConnectionStringBuilder
    $builder['Database'] = 'sqldatabasetest'
    $builder['Username'] = 'postgres'
    $builder['Password'] = 'qwerty'
    $builder['Timeout'] = 5

    $builder['Host'] = 'localhost'
    $builder['Port'] = $container.port
    $connectionString = $builder.ToString()
    
    $builder['Host'] = $container.ip
    $builder['Port'] = 5432
    $remoteConnectionString = $builder.ToString()

    @{
        containerId            = $container.containerId
        connectionString       = $connectionString
        remoteConnectionString = $remoteConnectionString
    }
}