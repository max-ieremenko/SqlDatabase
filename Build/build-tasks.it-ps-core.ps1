param(
    $settings
    , $database
    , $image
)

task Test StartDatabase, RunTest

Get-ChildItem -Path (Join-Path $PSScriptRoot 'scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

$containerId = ""
$connectionString = ""

Enter-Build {
    Write-Output "$database on $image"
}

task StartDatabase {
    $info = & "Start-$database"

    $script:containerId = $info.containerId
    $script:connectionString = $info.remoteConnectionString

    Write-Output $connectionString
    & "Wait-$database" $info.connectionString
}

task RunTest {
    $app = $settings.artifactsPowerShell + ":/root/.local/share/powershell/Modules/SqlDatabase"
    $test = (Join-Path $settings.integrationTests $database) + ":/test"

    exec {
        docker run --rm `
            -v $app `
            -v $test `
            --env connectionString=$connectionString `
            $image `
            pwsh -Command ./test/TestPowerShell.ps1
    }
}

Exit-Build {
    if ($containerId) {
        exec { docker container rm -f $containerId } | Out-Null
    }
}