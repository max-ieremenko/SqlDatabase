param(
    $settings
    , $database
    , $image
)

task Default StartDatabase, RunTest

. (Join-Path $PSScriptRoot '../scripts/Import-All.ps1')

$containerId = ''
$connectionString = ''

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
    $packageVersion = $settings.version
    $packageName = "SqlDatabase.GlobalTool.$packageVersion.nupkg"
    $app = (Join-Path $settings.artifacts $packageName) + ":/app/$packageName"
    $test = (Join-Path $settings.integrationTests $database) + ':/test'

    exec {
        docker run -it --rm `
            -v $app `
            -v $test `
            --env connectionString=$connectionString `
            --env test=/test `
            --env app=/app `
            --env packageVersion=$packageVersion `
            $image `
            bash /test/TestGlobalTool.sh
    }
}

Exit-Build {
    if ($containerId) {
        exec { docker container rm -f $containerId } | Out-Null
    }
}