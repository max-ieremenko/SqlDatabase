param(
    $settings,
    $targetFramework
)

task Test RunContainers, UpdateConfig, RunTests

. .\build-scripts.ps1

$mssqlContainerId = "empty"
$mssqlConnectionString = "empty"
$pgsqlContainerId = "empty"
$pgsqlConnectionString = "empty"
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

    Wait-Mssql $mssqlConnectionString
    Wait-Pgsql $pgsqlConnectionString
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
    }
}

task RunTests {
    $testList = Get-ChildItem -Path $testDir -Filter *.Test.dll `
        | Where-Object FullName -NotMatch \\ref\\ `
        | ForEach-Object {$_.FullName}    
    
    if (-not $testList.Count) {
        throw "Test list is empty."
    }
    
    $testList
    exec { dotnet vstest $testList }
}


Exit-Build {
    exec { docker container rm -f $mssqlContainerId $pgsqlContainerId } | Out-Null
}