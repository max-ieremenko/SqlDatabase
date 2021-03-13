param(
    $settings,
    $targetFramework
)

task Test RunMssql, UnZip, RunTest

. .\build-scripts.ps1

$mssqlContainerId = ""
$connectionString = ""
$tempDir = Join-Path $settings.bin ([Guid]::NewGuid().ToString())

Enter-Build {
    Write-Output "$targetFramework"
}

task UnZip {
    New-Item -Path $tempDir -ItemType Directory | Out-Null

    $package = Join-Path $settings.artifacts "SqlDatabase.$($settings.version)-$targetFramework.zip"
    Expand-Archive -Path $package -DestinationPath $tempDir
}

task RunMssql {
    $info = Start-Mssql

    $script:mssqlContainerId = $info.containerId
    $script:connectionString = $info.connectionString

    Write-Output $connectionString
}

task RunTest {
    Wait-Mssql $info.connectionString

    $app = Join-Path $tempDir "SqlDatabase.exe"
    $script = Join-Path $settings.integrationTests "Test.ps1"

    & $script $app $connectionString
}

Exit-Build {
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Force -Recurse
    }

    if ($mssqlContainerId) {
        exec { docker container rm -f $mssqlContainerId } | Out-Null
    }
}