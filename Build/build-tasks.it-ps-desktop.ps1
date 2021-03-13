param(
    $settings
)

task Test RunMssql, CopyModule, PublishModule, RunTest

. .\build-scripts.ps1

$mssqlContainerId = ""
$connectionString = ""
$testDir = Join-Path ([Environment]::GetFolderPath("MyDocuments")) "WindowsPowerShell\modules\SqlDatabase"

task CopyModule {
    if (Test-Path $testDir) {
        Remove-Item -Path $testDir -Force -Recurse
    }

    Copy-Item -Path $settings.artifactsPowerShell -Destination $testDir -Force -Recurse
}

task PublishModule {
    $log = Join-Path $settings.artifacts "Publish-Module.whatif.log"
    $command = "Publish-Module -Name SqlDatabase -WhatIf -Verbose -NuGetApiKey 123 *> $log"

    exec { powershell -NoLogo -Command "$command" }
}

task RunMssql {
    $info = Start-Mssql

    $script:mssqlContainerId = $info.containerId
    $script:connectionString = $info.connectionString

    Write-Output $connectionString
}

task RunTest {
    Wait-Mssql $connectionString

    $env:connectionString = $connectionString

    $testScript = Join-Path $settings.integrationTests "TestPowerShell.ps1"
    $command = ". $testScript"

    exec { powershell -NoLogo -Command "$command" }
}

Exit-Build {
    if (Test-Path $testDir) {
        Remove-Item -Path $testDir -Force -Recurse
    }

    if ($mssqlContainerId) {
        exec { docker container rm -f $mssqlContainerId } | Out-Null
    }
}