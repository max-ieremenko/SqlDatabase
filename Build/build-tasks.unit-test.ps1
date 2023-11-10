param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ })]
    [string]
    $Sources,

    [Parameter(Mandatory)]
    [ValidateSet("net472", "net6.0", "net7.0")] 
    [string]
    $Framework
)

task Default RunContainers, UpdateConfig, RunTests

Get-ChildItem -Path (Join-Path $PSScriptRoot 'scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

$containerIds = @()
$mssqlConnectionString = ""
$pgsqlConnectionString = ""
$mysqlConnectionString = ""

Enter-Build {
    $testList = Get-ChildItem -Path $Sources -Recurse -Filter *.Test.dll `
    | Where-Object FullName -Match \\$Framework\\ `
    | Where-Object FullName -Match \\bin\\Release\\ `
    | Where-Object FullName -NotMatch \\$Framework\\ref\\ `
    | ForEach-Object { $_.FullName }

    if (-not $testList) {
        throw "Test list is empty."
    }
    
    $testList
}

Exit-Build {
    if ($containerIds) {
        exec { docker container rm -f $containerIds } | Out-Null
    }
}

task RunContainers {
    $info = Start-Mssql
    $script:containerIds += $info.containerId
    $script:mssqlConnectionString = $info.connectionString
    Write-Output $mssqlConnectionString

    $info = Start-Pgsql
    $script:containerIds += $info.containerId
    $script:pgsqlConnectionString = $info.connectionString
    Write-Output $pgsqlConnectionString

    $info = Start-Mysql
    $script:containerIds += $info.containerId
    $script:mysqlConnectionString = $info.connectionString
    Write-Output $mysqlConnectionString

    Wait-Mssql $mssqlConnectionString
    Wait-Pgsql $pgsqlConnectionString
    Wait-Mysql $mysqlConnectionString
}

task UpdateConfig {
    $configFiles = $testList | ForEach-Object { Split-Path -Path $_ -Parent } | Sort-Object | Get-Unique | Get-ChildItem -Filter *.dll.config
    foreach ($configFile in $configFiles) {
        [xml]$config = Get-Content $configFile

        $node = $config.SelectSingleNode("configuration/connectionStrings/add[@name = 'mssql']")
        if ($node) {
            $node.Attributes["connectionString"].InnerText = $mssqlConnectionString
            $config.Save($configFile)
        }

        $node = $config.SelectSingleNode("configuration/connectionStrings/add[@name = 'pgsql']")
        if ($node) {
            $node.Attributes["connectionString"].InnerText = $pgsqlConnectionString
            $config.Save($configFile)
        }

        $node = $config.SelectSingleNode("configuration/connectionStrings/add[@name = 'mysql']")
        if ($node) {
            $node.Attributes["connectionString"].InnerText = $mysqlConnectionString
            $config.Save($configFile)
        }
    }
}

task RunTests {
    exec { dotnet vstest $testList }
}
