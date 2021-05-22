param(
    $settings
)

task Test RunContainers, CopyModule, PublishModule, RunTest

. .\build-scripts.ps1

$mssqlContainerId = ""
$mssqlConnectionString = ""
$pgsqlContainerId = ""
$pgsqlConnectionString = ""
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

task RunContainers {
    $info = Start-Mssql
    $script:mssqlContainerId = $info.containerId
    $script:mssqlConnectionString = $info.connectionString
    Write-Output $mssqlConnectionString

    $info = Start-Pgsql
    $script:pgsqlContainerId = $info.containerId
    $script:pgsqlConnectionString = $info.connectionString
    Write-Output $pgsqlConnectionString
}

task RunTest {
    Wait-Mssql $mssqlConnectionString
    Wait-Pgsql $pgsqlConnectionString

    $env:connectionString = $mssqlConnectionString
    $testScript = Join-Path $settings.integrationTests "MsSql\TestPowerShell.ps1"
    $command = ". $testScript"
    exec { powershell -NoLogo -Command "$command" }

    $env:connectionString = $pgsqlConnectionString
    $testScript = Join-Path $settings.integrationTests "PgSql\TestPowerShell.ps1"
    $command = ". $testScript"
    exec { powershell -NoLogo -Command "$command" }
}

Exit-Build {
    if (Test-Path $testDir) {
        Remove-Item -Path $testDir -Force -Recurse
    }

    exec { docker container rm -f $mssqlContainerId $pgsqlContainerId } | Out-Null
}