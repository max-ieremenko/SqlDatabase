param(
    $settings,
    $image
)

task Test RunMssql, RunTest

. .\build-scripts.ps1

$mssqlContainerId = ""
$connectionString = ""

Enter-Build {
    Write-Output "$image"
}

task RunMssql {
    $info = Start-Mssql

    $script:mssqlContainerId = $info.containerId
    $script:connectionString = $info.remoteConnectionString

    Write-Output $connectionString
    Wait-Mssql $info.connectionString
}

task RunTest {
    $packageVersion = $settings.version
    $packageName = "SqlDatabase.GlobalTool.$packageVersion.nupkg"
    $app = (Join-Path $settings.artifacts $packageName) + ":/app/$packageName"
    $test = $settings.integrationTests + ":/test"

    exec {
        docker run --rm `
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
    if ($mssqlContainerId) {
        exec { docker container rm -f $mssqlContainerId } | Out-Null
    }
}