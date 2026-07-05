param(
    [Parameter()]
    [string]
    $GithubToken
)

task GithubBuild Initialize, Clean, Build, ThirdPartyNotices, Pack
task LocalBuild GithubBuild, UnitTest, IntegrationTest

task Pack PackGlobalTool, PackPoweShellModule, PackNuget472, PackManualDownload
task IntegrationTest InitializeIntegrationTest, PsDesktopTest, PsCoreTest, SdkToolTest, NetRuntimeLinuxTest, NetRuntimeWindowsTest

Get-ChildItem -Path (Join-Path $PSScriptRoot '../scripts') -Filter *.ps1 | ForEach-Object { . $_.FullName }

task Initialize {
    $root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\'))
    $sources = Join-Path $root 'Sources'
    $bin = Join-Path $root 'bin'
    $artifacts = Join-Path $bin 'artifacts'

    $script:settings = @{
        sources             = $sources
        bin                 = $bin
        artifacts           = $artifacts
        artifactsPowerShell = Join-Path $artifacts 'PowerShell'
        integrationTests    = Join-Path $bin 'IntegrationTests'
        version             = Get-ReleaseVersion -SourcePath $sources
        githubToken         = $GithubToken
        repositoryCommitId  = git rev-parse HEAD
    }

    $script:frameworks = 'net472', 'net8.0', 'net9.0', 'net10.0'
    $script:databases = 'MsSql', 'PgSql', 'MySql'

    Write-Output "PackageVersion: $($settings.version)"
    Write-Output "CommitId: $($settings.repositoryCommitId)"

    if ($IsWindows) {
        Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
    }
}

task Clean {
    Remove-DirectoryRecurse -Path $settings.bin
    Remove-DirectoryRecurse -Path $settings.sources -Filters 'bin', 'obj'

    New-Item -Path $settings.bin -ItemType Directory | Out-Null
}

task Build {
    $solutionFile = Join-Path $settings.sources 'SqlDatabase.slnx'
    exec { dotnet restore $solutionFile }
    exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Release }
}

task ThirdPartyNotices {
    Invoke-Build -File 'build-tasks.third-party.ps1' -settings $settings
}

task PackGlobalTool {
    $projectFile = Join-Path $settings.sources 'SqlDatabase\SqlDatabase.csproj'

    exec {
        dotnet pack `
            -c Release `
            -p:PackAsTool=true `
            -p:GlobalTool=true `
            -o $($settings.artifacts) `
            $projectFile
    }
}

task PackPoweShellModule {
    $source = Join-Path $settings.bin 'SqlDatabase.PowerShell'
    $dest = $settings.artifactsPowerShell
    
    Copy-Item -Path $source -Destination $dest -Recurse

    # .psd1 set module version
    $psdFile = Join-Path $dest 'SqlDatabase.psd1'
    ((Get-Content -Path $psdFile -Raw) -replace '1.2.3', $settings.version) | Set-Content -Path $psdFile

    # copy license
    Copy-Item -Path (Join-Path $settings.sources '..\LICENSE.md') -Destination $dest

    # copy ThirdPartyNotices
    Copy-Item -Path (Join-Path $settings.bin 'ThirdPartyNotices.txt') -Destination $dest

    Get-ChildItem $dest -Include *.pdb -Recurse | Remove-Item
}

task PackNuget472 PackPoweShellModule, {
    $bin = $settings.artifactsPowerShell
    if (-not $bin.EndsWith('\')) {
        $bin += '\'
    }

    $nuspec = Join-Path $settings.sources 'SqlDatabase.Package\nuget\package.nuspec'
    exec {
        dotnet pack `
            --no-build `
            --version=$($settings.version) `
            -p:RepositoryCommit=$($settings.repositoryCommitId) `
            -p:bin=$bin `
            -o $($settings.artifacts) `
            $nuspec
    }
}

task PackManualDownload PackGlobalTool, PackPoweShellModule, {
    Get-ChildItem -Path $settings.bin -Recurse -Directory -Filter 'publish' | Remove-Item -Force -Recurse
    Get-ChildItem -Path $settings.bin -Recurse -Directory -Filter 'win-arm64' | Remove-Item -Force -Recurse

    $out = $settings.artifacts
    $lic = Join-Path $settings.sources '..\LICENSE.md'
    $thirdParty = Join-Path $settings.bin 'ThirdPartyNotices.txt'
    $packageVersion = $settings.version

    $destination = Join-Path $out "SqlDatabase.$packageVersion-PowerShell.zip"
    $source = Join-Path $settings.artifactsPowerShell '*'
    Compress-Archive -Path $source -DestinationPath $destination

    foreach ($target in $frameworks) {
        $destination = Join-Path $out "SqlDatabase.$packageVersion-$target.zip"
        $source = Join-Path $settings.bin "SqlDatabase\$target\*"
        Compress-Archive -Path $source, $lic, $thirdParty -DestinationPath $destination
    }
}

task UnitTest {
    $builds = @()
    foreach ($case in $frameworks) {
        $builds += @{
            File      = 'build-tasks.unit-test.ps1'
            Sources   = $settings.sources
            Framework = $case
        }
    }
    
    Build-Parallel $builds -ShowParameter Framework -MaximumBuilds 4
}

task InitializeIntegrationTest {
    $dest = $settings.integrationTests
    if (Test-Path $dest) {
        Remove-Item -Path $dest -Force -Recurse
    }

    Copy-Item -Path (Join-Path $settings.sources 'IntegrationTests') -Destination $dest -Force -Recurse
    $assemblyScript = Join-Path $settings.bin '..\Examples\CSharpMirationStep\bin\Release\net472\2.1_2.2.*'
    foreach ($database in $databases) {
        Copy-Item -Path $assemblyScript -Destination (Join-Path $dest "$database\Upgrade") -Force
    }

    $bashLine = "sed -i 's/\r//g'"
    foreach ($database in $databases) {
        $bashLine += " test/$database/TestGlobalTool.sh test/$database/Test.sh"
    }

    # fix unix line endings
    $test = $dest + ':/test'
    exec {
        docker run --rm `
            -v $test `
            mcr.microsoft.com/dotnet/sdk:8.0 `
            bash -c $bashLine
    }
}

task PsDesktopTest {
    foreach ($database in $databases) {
        Invoke-Build -File 'build-tasks.it-ps-desktop.ps1' -settings $settings -database MsSql
    }
}

task PsCoreTest {
    # show-powershell-releases.ps1
    $versions = '7.2.0', '7.3.0', '7.4.0', '7.4.17', '7.5.0', '7.5.8', '7.6.0', '7.6.3', '7.7.0-preview.2'

    $builds = @()
    foreach ($version in $versions) {
        foreach ($database in $databases) {
            $builds += @{
                File     = 'build-tasks.it-ps-core.ps1'
                settings = $settings
                database = $database
                image    = "sqldatabase/pwsh:$version"
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, image -MaximumBuilds 4
}

task SdkToolTest {
    $versions = '8.0', '9.0', '10.0'

    $builds = @()
    foreach ($version in $versions) {
        foreach ($database in $databases) {
            $builds += @{
                File     = 'build-tasks.it-tool-linux.ps1'
                settings = $settings
                database = $database
                image    = "sqldatabase/dotnet_pwsh:$version-sdk"
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, image -MaximumBuilds 4
}

task NetRuntimeLinuxTest {
    $versions = '8.0', '9.0', '10.0'

    $builds = @()
    foreach ($version in $versions) {
        foreach ($database in $databases) {
            $builds += @{
                File            = 'build-tasks.it-linux.ps1'
                settings        = $settings
                targetFramework = "net$version"
                database        = $database
                image           = "sqldatabase/dotnet_pwsh:$version-runtime"
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, targetFramework, image -MaximumBuilds 4
}

task NetRuntimeWindowsTest {
    $builds = @()
    foreach ($case in $frameworks) {
        foreach ($database in $databases) {
            $builds += @{
                File            = 'build-tasks.it-win.ps1'
                settings        = $settings
                targetFramework = $case
                database        = $database
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, targetFramework -MaximumBuilds 4
}