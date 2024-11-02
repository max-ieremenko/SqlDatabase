function Start-Mssql {
    param ()

    $container = Start-Container -Image sqldatabase/mssql:2017 -ContainerPort 1433
    $port = $container.port

    $builder = New-Object -TypeName System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Initial Catalog'] = 'SqlDatabaseTest'
    $builder['User Id'] = 'sa'
    $builder['Password'] = 'P@ssw0rd'
    $builder['Connect Timeout'] = 5

    $builder['Data Source'] = ".,$port"
    $connectionString = $builder.ToString()
    
    $builder['Data Source'] = $container.ip
    $remoteConnectionString = $builder.ToString()

    return @{
        containerId            = $container.containerId
        connectionString       = $connectionString
        remoteConnectionString = $remoteConnectionString
    }
}