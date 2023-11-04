param(
    $settings,
    $targetFramework
)

task Test RunContainers, UpdateConfig, RunTests

Get-ChildItem -Path (Join-Path $PSScriptRoot 'scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

$mssqlContainerId = ""
$mssqlConnectionString = "empty"
$pgsqlContainerId = ""
$pgsqlConnectionString = "empty"
$mysqlContainerId = ""
$mysqlConnectionString = "empty"
$testDir = Join-Path (Join-Path $settings.bin "Tests") $targetFramework

Enter-Build {
    Write-Output "$testDir"
}

task RunContainers {
    $info = Start-Mssql
    $script:mssqlContainerId = $info.containerId
    $script:mssqlConnectionString = $info.connectionString
    Write-Output $mssqlConnectionString

    $info = Start-Pgsql
    $script:pgsqlContainerId = $info.containerId
    $script:pgsqlConnectionString = $info.connectionString
    Write-Output $pgsqlConnectionString

    $info = Start-Mysql
    $script:mysqlContainerId = $info.containerId
    $script:mysqlConnectionString = $info.connectionString
    Write-Output $mysqlConnectionString

    Wait-Mssql $mssqlConnectionString
    Wait-Pgsql $pgsqlConnectionString
    Wait-Mysql $mysqlConnectionString
}


task UpdateConfig {
    $configFiles = Get-ChildItem -Path $testDir -Filter *.dll.config
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
    $testList = Get-ChildItem -Path $testDir -Filter *.Test.dll `
        | Where-Object FullName -NotMatch \\ref\\ `
        | ForEach-Object {$_.FullName}    
    
    if (-not $testList) {
        throw "Test list is empty."
    }
    
    $testList
    exec { dotnet vstest $testList }
}


Exit-Build {
    exec { docker container rm -f $mssqlContainerId $pgsqlContainerId $mysqlContainerId } | Out-Null
}