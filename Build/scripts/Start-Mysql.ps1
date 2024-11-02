function Start-Mysql {
    param ()
    
    $sqlConnectordll = Join-Path $env:USERPROFILE '\.nuget\packages\mysqlconnector\1.3.10\lib\netstandard2.0\MySqlConnector.dll'
    Add-Type -Path $sqlConnectordll

    $container = Start-Container -Image sqldatabase/mysql:8.0.25 -ContainerPort 3306

    $builder = New-Object -TypeName MySqlConnector.MySqlConnectionStringBuilder
    $builder['Database'] = 'sqldatabasetest'
    $builder['User ID'] = 'root'
    $builder['Password'] = 'qwerty'
    $builder['ConnectionTimeout'] = 5

    $builder.Server = 'localhost'
    $builder.Port = $container.port.ToString()
    $connectionString = $builder.ToString()
    
    $builder.Server = $container.ip.ToString()
    $builder.Port = 3306
    $remoteConnectionString = $builder.ToString()

    return @{
        containerId            = $container.containerId
        connectionString       = $connectionString
        remoteConnectionString = $remoteConnectionString
    }
}