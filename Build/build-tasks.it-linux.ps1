param(
    $settings,
    $targetFramework,
    $image
)

task Test RunMssql, UnZip, RunTest

. .\build-scripts.ps1

$mssqlContainerId = ""
$connectionString = ""
$remoteConnectionString = ""
$tempDir = Join-Path $settings.bin ([Guid]::NewGuid().ToString())

Enter-Build {
    Write-Output "$image"
}

task UnZip {
    New-Item -Path $tempDir -ItemType Directory | Out-Null

    $package = Join-Path $settings.artifacts "SqlDatabase.$($settings.version)-$targetFramework.zip"
    Expand-Archive -Path $package -DestinationPath $tempDir
}

task RunMssql {
    $info = Start-Mssql

    $script:mssqlContainerId = $info.containerId
    $script:remoteConnectionString = $info.remoteConnectionString
    $script:connectionString = $info.connectionString

    Write-Output $connectionString
}

task RunTest {
    Wait-Mssql $info.connectionString

    $app = $tempDir + ":/app"
    $test = $settings.integrationTests + ":/test"

    exec {
        docker run --rm `
            -v $app `
            -v $test `
            --env connectionString=$remoteConnectionString `
            --env test=/test `
            -w "/app" `
            $image `
            bash /test/Test.sh
    }
}

Exit-Build {
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Force -Recurse
    }

    if ($mssqlContainerId) {
        exec { docker container rm -f $mssqlContainerId } | Out-Null
    }
}