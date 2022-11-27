function Get-AssemblyVersion($assemblyInfoCsPath) {
    $Anchor = "AssemblyVersion(""";
    $lines = Get-Content -Path $assemblyInfoCsPath

    foreach ($line in $lines) {
        $index = $line.IndexOf($Anchor);
        if ($index -lt 0) {
            continue;
        }
        
        $text = $line.Substring($index + $Anchor.Length);

        $index = $text.IndexOf('"');
        $text = $text.Substring(0, $index);
    
        $version = New-Object -TypeName System.Version -ArgumentList $text
        $build = $version.Build
        if ($build -le 0) {
            $build = 0
        }
    
        $text = (New-Object -TypeName System.Version -ArgumentList $version.Major, $version.Minor, $build).ToString();
        return $text;
    }
}

function Get-RepositoryCommitId {
    git rev-parse HEAD
}

function Start-Mssql {
    $container = Start-Container sqldatabase/mssql:2017 1433
    $port = $container.port

    $builder = New-Object -TypeName System.Data.SqlClient.SqlConnectionStringBuilder
    $builder["Initial Catalog"] = "SqlDatabaseTest"
    $builder["User Id"] = "sa"
    $builder["Password"] = "P@ssw0rd"
    $builder["Connect Timeout"] = 5

    $builder["Data Source"] = ".,$port"
    $connectionString = $builder.ToString()
    
    $builder["Data Source"] = $container.ip
    $remoteConnectionString = $builder.ToString()

    return @{
        containerId            = $container.containerId
        connectionString       = $connectionString
        remoteConnectionString = $remoteConnectionString
    }
}

function Wait-Mssql($connectionString) {
    Wait-Connection System.Data.SqlClient.SqlConnection $connectionString
}

function Start-Pgsql {
    $npgsqldll = Join-Path $env:USERPROFILE ".nuget\packages\npgsql\4.0.11\lib\netstandard2.0\Npgsql.dll"
    Add-Type -Path $npgsqldll

    $container = Start-Container sqldatabase/postgres:13.3 5432

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

function Wait-Pgsql($connectionString) {
    Wait-Connection Npgsql.NpgsqlConnection $connectionString
}

function Start-Mysql {
    $sqlConnectordll = Join-Path $env:USERPROFILE "\.nuget\packages\mysqlconnector\1.3.10\lib\netstandard2.0\MySqlConnector.dll"
    Add-Type -Path $sqlConnectordll

    $container = Start-Container sqldatabase/mysql:8.0.25 3306

    $builder = New-Object -TypeName MySqlConnector.MySqlConnectionStringBuilder
    $builder["Database"] = "sqldatabasetest"
    $builder["User ID"] = "root"
    $builder["Password"] = "qwerty"
    $builder["ConnectionTimeout"] = 5

    $builder.Server = "localhost"
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

function Wait-Mysql($connectionString) {
    Wait-Connection MySqlConnector.MySqlConnection $connectionString
}

function Start-Container {
    param (
        $image,
        $containerPort
    )

    $containerId = exec { 
        docker run `
            -d `
            -p $containerPort `
            $image
    }
    
    $ip = exec { 
        docker inspect `
            --format "{{.NetworkSettings.Networks.bridge.IPAddress}}"  `
            $containerId
    }

    $hostPort = exec { 
        docker inspect `
            --format "{{(index (index .NetworkSettings.Ports \""$containerPort/tcp\"") 0).HostPort}}"  `
            $containerId
    }

    return @{
        containerId = $containerId
        ip          = $ip
        port        = $hostPort
    }
}

function Wait-Connection {
    param (
        $connectionName,
        $connectionString,
        $timeout = 50
    )

    function Test-Connection {
        $connection = New-Object -TypeName $connectionName -ArgumentList $connectionString
        try {
            $connection.Open()
        }
        finally {
            $connection.Dispose()
        }
    }

    for ($i = 0; $i -lt $timeout; $i++) {
        try {
            Test-Connection
            return
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    try {
        Test-Connection
    }
    catch {
        throw "$connectionName $connectionString"
    }
}