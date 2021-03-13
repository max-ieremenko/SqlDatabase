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
    $app = $settings.artifactsPowerShell + ":/root/.local/share/powershell/Modules/SqlDatabase"
    $test = $settings.integrationTests + ":/test"

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
    if ($mssqlContainerId) {
        exec { docker container rm -f $mssqlContainerId } | Out-Null
    }
}