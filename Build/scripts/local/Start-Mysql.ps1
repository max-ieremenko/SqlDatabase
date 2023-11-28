function Start-Mysql {
    param ()
    
    $container = Start-Container -Image sqldatabase/mysql:8.0.25 -ContainerPort 3306

    $builder = New-Object -TypeName System.Data.Common.DbConnectionStringBuilder
    $builder['Database'] = 'sqldatabasetest'
    $builder['User ID'] = 'root'
    $builder['Password'] = 'qwerty'
    $builder['Connection Timeout'] = 5

    $builder['Server'] = 'localhost'
    $builder['Port'] = $container.port
    $connectionString = $builder.ToString()
    
    $builder['Server'] = $container.ip
    $builder['Port'] = 3306
    $remoteConnectionString = $builder.ToString()

    @{
        containerId            = $container.containerId
        connectionString       = $connectionString
        remoteConnectionString = $remoteConnectionString
    }
}