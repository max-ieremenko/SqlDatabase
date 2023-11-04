param(
    $settings
    , $database
)

task Test RunContainers, CopyModule, PublishModule, RunTest

Get-ChildItem -Path (Join-Path $PSScriptRoot 'scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

$containerId = ""
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

task RunContainers {
    $info = & "Start-$database"
    $script:containerId = $info.containerId
    $script:connectionString = $info.connectionString
    Write-Output $connectionString
}

task RunTest {
    & "Wait-$database" $connectionString

    $env:connectionString = $connectionString
    $testScript = Join-Path $settings.integrationTests "$database\TestPowerShell.ps1"
    $command = ". $testScript"
    exec { powershell -NoLogo -Command "$command" }
}

Exit-Build {
    if (Test-Path $testDir) {
        Remove-Item -Path $testDir -Force -Recurse
    }

    exec { docker container rm -f $containerId } | Out-Null
}