param(
    $settings,
    $targetFramework
)

task Test RunMssql, UpdateConfig, RunTests

. .\build-scripts.ps1

$mssqlContainerId = "empty"
$connectionString = "empty"
$testDir = Join-Path (Join-Path $settings.bin "Tests") $targetFramework

Enter-Build {
    Write-Output "$testDir"
}

task RunMssql {
    $info = Start-Mssql

    $script:mssqlContainerId = $info.containerId
    $script:connectionString = $info.connectionString

    Write-Output $connectionString
    Wait-Mssql $connectionString
}

task UpdateConfig {
    $configFiles = Get-ChildItem -Path $testDir -Filter *.dll.config
    foreach ($configFile in $configFiles) {
        [xml]$config = Get-Content $configFile
        $node = $config.SelectSingleNode("configuration/connectionStrings/add[@name = 'test']")
        if ($node) {
            $node.Attributes["connectionString"].InnerText = $connectionString
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
    exec { docker container rm -f $mssqlContainerId } | Out-Null
}