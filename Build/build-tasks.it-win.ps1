param(
    $settings
    , $targetFramework
    , $database
)

task Default StartDatabase, UnZip, RunTest

Get-ChildItem -Path (Join-Path $PSScriptRoot 'scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

$containerId = ""
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

task StartDatabase {
    $info = & "Start-$database"

    $script:containerId = $info.containerId
    $script:connectionString = $info.connectionString

    Write-Output $connectionString
}

task RunTest {
    & "Wait-$database" $connectionString

    $app = Join-Path $tempDir "SqlDatabase.exe"
    $script = (Join-Path $settings.integrationTests $database)
    $script = Join-Path $script "Test.ps1"

    & $script $app $connectionString
}

Exit-Build {
    if (Test-Path $tempDir) {
        Remove-Item -Path $tempDir -Force -Recurse
    }

    if ($containerId) {
        exec { docker container rm -f $containerId } | Out-Null
    }
}