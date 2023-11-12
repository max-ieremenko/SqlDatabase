param(
    $settings
    , $targetFramework
    , $database
    , $image
)

task Default StartDatabase, UnZip, RunTest

Get-ChildItem -Path (Join-Path $PSScriptRoot '../scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

$containerId = ""
$connectionString = ""
$remoteConnectionString = ""
$tempDir = Join-Path $settings.bin ([Guid]::NewGuid().ToString())

Enter-Build {
    Write-Output "$database on $targetFramework on $image"
}

task UnZip {
    New-Item -Path $tempDir -ItemType Directory | Out-Null

    $package = Join-Path $settings.artifacts "SqlDatabase.$($settings.version)-$targetFramework.zip"
    Expand-Archive -Path $package -DestinationPath $tempDir
}

task StartDatabase {
    $info = & "Start-$database"

    $script:containerId = $info.containerId
    $script:remoteConnectionString = $info.remoteConnectionString
    $script:connectionString = $info.connectionString

    Write-Output $connectionString
}

task RunTest {
    & "Wait-$database" $connectionString

    $app = $tempDir + ":/app"
    $test = (Join-Path $settings.integrationTests $database) + ":/test"

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

    if ($containerId) {
        exec { docker container rm -f $containerId } | Out-Null
    }
}