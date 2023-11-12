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
    $root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\"))
    $sources = Join-Path $root "Sources"
    $bin = Join-Path $root "bin"
    $artifacts = Join-Path $bin "artifacts"

    $script:settings = @{
        nugetexe            = Join-Path $root "Build\nuget.exe"
        sources             = $sources
        bin                 = $bin
        artifacts           = $artifacts
        artifactsPowerShell = Join-Path $artifacts "PowerShell"
        integrationTests    = Join-Path $bin "IntegrationTests"
        version             = Get-ReleaseVersion -SourcePath $sources
        githubToken         = $GithubToken
        repositoryCommitId  = git rev-parse HEAD
    }

    $script:frameworks = "net472", "net6.0", "net7.0"
    $script:databases = "MsSql", "PgSql", "MySql"

    Write-Output "PackageVersion: $($settings.version)"
    Write-Output "CommitId: $($settings.repositoryCommitId)"
}

task Clean {
    Remove-DirectoryRecurse -Path $settings.bin
    Remove-DirectoryRecurse -Path $settings.sources -Filters "bin", "obj"

    New-Item -Path $settings.bin -ItemType Directory | Out-Null
}

task Build {
    $solutionFile = Join-Path $settings.sources "SqlDatabase.sln"
    exec { dotnet restore $solutionFile }
    exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Release }
}

task ThirdPartyNotices {
    Invoke-Build -File "build-tasks.third-party.ps1" -settings $settings
}

task PackGlobalTool {
    $projectFile = Join-Path $settings.sources "SqlDatabase\SqlDatabase.csproj"

    exec {
        dotnet pack `
            -c Release `
            -p:PackAsTool=true `
            -p:GlobalTool=true `
            -p:PackageVersion=$($settings.version) `
            -p:RepositoryCommit=$($settings.repositoryCommitId) `
            -o $($settings.artifacts) `
            $projectFile
    }
}

task PackPoweShellModule {
    $source = Join-Path $settings.bin "SqlDatabase.PowerShell\netstandard2.0\"
    $dest = $settings.artifactsPowerShell
    
    Copy-Item -Path $source -Destination $dest -Recurse

    # .psd1 set module version
    $psdFile = Join-Path $dest "SqlDatabase.psd1"
    ((Get-Content -Path $psdFile -Raw) -replace '1.2.3', $settings.version) | Set-Content -Path $psdFile

    # copy license
    Copy-Item -Path (Join-Path $settings.sources "..\LICENSE.md") -Destination $dest

    # copy ThirdPartyNotices
    Copy-Item -Path (Join-Path $settings.bin "ThirdPartyNotices.txt") -Destination $dest

    Get-ChildItem $dest -Include *.pdb -Recurse | Remove-Item
}

task PackNuget472 PackPoweShellModule, {
    $bin = $settings.artifactsPowerShell
    if (-not $bin.EndsWith("\")) {
        $bin += "\"
    }

    $nuspec = Join-Path $settings.sources "SqlDatabase.Package\nuget\package.nuspec"
    exec { 
        & $($settings.nugetexe) pack `
            -NoPackageAnalysis `
            -verbosity detailed `
            -OutputDirectory $($settings.artifacts) `
            -Version $($settings.version) `
            -p RepositoryCommit=$($settings.repositoryCommitId) `
            -p bin=$bin `
            $nuspec
    }
}

task PackManualDownload PackGlobalTool, PackPoweShellModule, {
    Get-ChildItem -Path $settings.bin -Recurse -Directory -Filter "publish" | Remove-Item -Force -Recurse

    $out = $settings.artifacts
    $lic = Join-Path $settings.sources "..\LICENSE.md"
    $thirdParty = Join-Path $settings.bin "ThirdPartyNotices.txt"
    $packageVersion = $settings.version

    $destination = Join-Path $out "SqlDatabase.$packageVersion-PowerShell.zip"
    $source = Join-Path $settings.artifactsPowerShell "*"
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
            File      = "build-tasks.unit-test.ps1"
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

    Copy-Item -Path (Join-Path $settings.sources "IntegrationTests") -Destination $dest -Force -Recurse
    $assemblyScript = Join-Path $settings.bin "..\Examples\CSharpMirationStep\bin\Release\net472\2.1_2.2.*"
    foreach ($database in $databases) {
        Copy-Item -Path $assemblyScript -Destination (Join-Path $dest "$database\Upgrade") -Force
    }

    $bashLine = "sed -i 's/\r//g'"
    foreach ($database in $databases) {
        $bashLine += " test/$database/TestGlobalTool.sh test/$database/Test.sh"
    }

    # fix unix line endings
    $test = $dest + ":/test"
    exec {
        docker run --rm `
            -v $test `
            mcr.microsoft.com/dotnet/sdk:7.0 `
            bash -c $bashLine
    }
}

task PsDesktopTest {
    $builds = @()
    foreach ($database in $databases) {
        $builds += @{
            File     = "build-tasks.it-ps-desktop.ps1"
            settings = $settings
            database = $database
        }
    }

    Build-Parallel $builds -ShowParameter database -MaximumBuilds 1
}

task PsCoreTest {
    # show-powershell-images.ps1
    $images = $(
        "mcr.microsoft.com/powershell:6.1.0-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.1.1-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.1.2-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.1.3-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.2.0-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.2.1-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.2.2-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.2.3-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.2.4-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.0.0-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.0.1-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.0.2-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.0.3-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.1.0-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.1.1-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.1.2-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.1.3-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.1.4-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.2.0-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.2.1-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.2.2-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.3-ubuntu-20.04")

    $builds = @()
    foreach ($image in $images) {
        foreach ($database in $databases) {
            $builds += @{
                File     = "build-tasks.it-ps-core.ps1"
                settings = $settings
                database = $database
                image    = $image
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, image -MaximumBuilds 4
}

task SdkToolTest {
    $images = $(
        "sqldatabase/dotnet_pwsh:6.0-sdk"
        , "sqldatabase/dotnet_pwsh:7.0-sdk")

    $builds = @()
    foreach ($image in $images) {
        foreach ($database in $databases) {
            $builds += @{
                File     = "build-tasks.it-tool-linux.ps1"
                settings = $settings
                database = $database
                image    = $image
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, image -MaximumBuilds 4
}

task NetRuntimeLinuxTest {
    $testCases = $(
        @{ targetFramework = "net6.0"; image = "sqldatabase/dotnet_pwsh:6.0-runtime" }
        , @{ targetFramework = "net7.0"; image = "sqldatabase/dotnet_pwsh:7.0-runtime" }
    )

    $builds = @()
    foreach ($case in $testCases) {
        foreach ($database in $databases) {
            $builds += @{
                File            = "build-tasks.it-linux.ps1"
                settings        = $settings
                targetFramework = $case.targetFramework
                database        = $database
                image           = $case.image
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
                File            = "build-tasks.it-win.ps1"
                settings        = $settings
                targetFramework = $case
                database        = $database
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, targetFramework -MaximumBuilds 4
}