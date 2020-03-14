function Get-AssemblyVersion($assemblyInfoCsPath) {
    $Anchor = "AssemblyVersion(""";
    $lines = Get-Content -Path $assemblyInfoCsPath

    foreach ($line in $lines) {
        $index = $line.IndexOf($Anchor);
        if ($index -lt 0) {
            continue;
        }
        
        $text = $line.Substring($index + $Anchor.Length);

        $index = $text.IndexOf('"');
        $text = $text.Substring(0, $index);
    
        $version = New-Object -TypeName System.Version -ArgumentList $text
        $build = $version.Build
        if ($build -le 0) {
            $build = 0
        }
    
        $text = (New-Object -TypeName System.Version -ArgumentList $version.Major, $version.Minor, $build).ToString();
        return $text;
    }
}

function Get-RepositoryCommitId {
    $response = (Invoke-RestMethod -Uri "https://api.github.com/repos/max-ieremenko/SqlDatabase/commits/master")
    return $response.sha
}

function Resolve-SqlServerIp ($containerName) {
    return Exec { docker inspect --format='{{.NetworkSettings.Networks.docker_default.Gateway}}' $containerName }
}

function Test-PowerShellCore($image) {
    $app = $moduleBin + ":/root/.local/share/powershell/Modules/SqlDatabase"
    $test = $moduleIntegrationTests + ":/test"

    Exec {
        docker run --rm `
            -v $app `
            -v $test `
            --env connectionString=$connectionString `
            --env test=/test `
            $image `
            pwsh -Command ./test/Test.ps1
    }
}

function Test-PowerShellDesktop($command) {
    $app = Join-Path ([Environment]::GetFolderPath("MyDocuments")) "WindowsPowerShell\modules\SqlDatabase"

    if (Test-Path $app) {
        Remove-Item -Path $app -Force -Recurse
    }

    try {
        Copy-Item -Path $moduleBin -Destination $app -Force -Recurse
        Exec { powershell -NoLogo -Command "$command" }
    }
    finally {
        Remove-Item -Path $app -Force -Recurse
    }
}