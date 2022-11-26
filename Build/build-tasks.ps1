task Default Initialize, Clean, Build, ThirdPartyNotices, Pack, UnitTest, IntegrationTest
task Pack PackGlobalTool, PackPoweShellModule, PackNuget452, PackManualDownload
task IntegrationTest InitializeIntegrationTest, PsDesktopTest, PsCoreTest, SdkToolTest, NetRuntimeLinuxTest, NetRuntimeWindowsTest

. .\build-scripts.ps1

task Initialize {
    $sources = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\Sources"))
    $bin = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\bin"))
    $artifacts = Join-Path $bin "artifacts"

    $script:settings = @{
        nugetexe            = Join-Path $PSScriptRoot "nuget.exe";
        sources             = $sources;
        bin                 = $bin;
        artifacts           = $artifacts
        artifactsPowerShell = Join-Path $artifacts "PowerShell"
        integrationTests    = Join-Path $bin "IntegrationTests"
        version             = Get-AssemblyVersion (Join-Path $sources "GlobalAssemblyInfo.cs");
        repositoryCommitId  = Get-RepositoryCommitId;
    }

    $script:databases = $("MsSql", "PgSql", "MySql")

    Write-Output "PackageVersion: $($settings.version)"
    Write-Output "CommitId: $($settings.repositoryCommitId)"
}

task Clean {
    if (Test-Path $settings.bin) {
        Remove-Item -Path $settings.bin -Recurse -Force
    }

    New-Item -Path $settings.bin -ItemType Directory | Out-Null
}

task Build {
    $solutionFile = Join-Path $settings.sources "SqlDatabase.sln"
    exec { dotnet restore $solutionFile }
    exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Release }
}

task ThirdPartyNotices {
    Invoke-Build -File build-tasks.third-party.ps1 -settings $settings
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
    ((Get-Content -Path $psdFile -Raw) -replace '{{ModuleVersion}}', $settings.version) | Set-Content -Path $psdFile

    # copy license
    Copy-Item -Path (Join-Path $settings.sources "..\LICENSE.md") -Destination $dest

    # copy ThirdPartyNotices
    Copy-Item -Path (Join-Path $settings.bin "ThirdPartyNotices.txt") -Destination $dest

    Get-ChildItem $dest -Include *.pdb -Recurse | Remove-Item
}

task PackNuget452 PackPoweShellModule, {
    $bin = $settings.artifactsPowerShell
    if (-not $bin.EndsWith("\")) {
        $bin += "\"
    }

    $nuspec = Join-Path $settings.sources "SqlDatabase.Package\nuget\package.nuspec"
    Exec { 
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
    $out = $settings.artifacts
    $lic = Join-Path $settings.sources "..\LICENSE.md"
    $thirdParty = Join-Path $settings.bin "ThirdPartyNotices.txt"
    $packageVersion = $settings.version

    $destination = Join-Path $out "SqlDatabase.$packageVersion-net452.zip"
    $source = Join-Path $settings.bin "SqlDatabase\net452\*"
    Compress-Archive -Path $source, $lic, $thirdParty -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-PowerShell.zip"
    $source = Join-Path $settings.artifactsPowerShell "*"
    Compress-Archive -Path $source -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-netcore31.zip"
    $source = Join-Path $settings.bin "SqlDatabase\netcoreapp3.1\publish\*"
    Compress-Archive -Path $source, $lic, $thirdParty -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-net50.zip"
    $source = Join-Path $settings.bin "SqlDatabase\net5.0\publish\*"
    Compress-Archive -Path $source, $lic, $thirdParty -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-net60.zip"
    $source = Join-Path $settings.bin "SqlDatabase\net6.0\publish\*"
    Compress-Archive -Path $source, $lic, $thirdParty -DestinationPath $destination
}

task UnitTest {
    $builds = @(
        @{ File = "build-tasks.unit-test.ps1"; Task = "Test"; settings = $settings; targetFramework = "net472" }
        @{ File = "build-tasks.unit-test.ps1"; Task = "Test"; settings = $settings; targetFramework = "netcoreapp3.1" }
        @{ File = "build-tasks.unit-test.ps1"; Task = "Test"; settings = $settings; targetFramework = "net5.0" }
        @{ File = "build-tasks.unit-test.ps1"; Task = "Test"; settings = $settings; targetFramework = "net6.0" }
    )
    
    Build-Parallel $builds -ShowParameter targetFramework -MaximumBuilds 4
}

task InitializeIntegrationTest {
    $dest = $settings.integrationTests
    if (Test-Path $dest) {
        Remove-Item -Path $dest -Force -Recurse
    }

    Copy-Item -Path (Join-Path $settings.sources "SqlDatabase.Test\IntegrationTests") -Destination $dest -Force -Recurse
    foreach ($database in $databases) {
        Copy-Item -Path (Join-Path $settings.bin "Tests\net472\2.1_2.2.*") -Destination (Join-Path $dest "$database\Upgrade") -Force
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
            mcr.microsoft.com/dotnet/core/sdk:3.1 `
            bash -c $bashLine
    }
}

task PsDesktopTest {
    $builds = @()
    foreach ($database in $databases) {
        $builds += @{
            File     = "build-tasks.it-ps-desktop.ps1";
            Task     = "Test";
            settings = $settings;
            database = $database;
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
        , "mcr.microsoft.com/powershell:7.3.0-preview.1-ubuntu-20.04")

    $builds = @()
    foreach ($image in $images) {
        foreach ($database in $databases) {
            $builds += @{
                File     = "build-tasks.it-ps-core.ps1";
                Task     = "Test";
                settings = $settings;
                database = $database;
                image    = $image;
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, image -MaximumBuilds 4
}

task SdkToolTest {
    $images = $(
        "sqldatabase/dotnet_pwsh:3.1-sdk"
        , "sqldatabase/dotnet_pwsh:5.0-sdk"
        , "sqldatabase/dotnet_pwsh:6.0-sdk")

    $builds = @()
    foreach ($image in $images) {
        foreach ($database in $databases) {
            $builds += @{
                File     = "build-tasks.it-tool-linux.ps1";
                Task     = "Test";
                settings = $settings;
                database = $database;
                image    = $image;
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, image -MaximumBuilds 4
}

task NetRuntimeLinuxTest {
    $testCases = $(
        @{ targetFramework = "netcore31"; image = "sqldatabase/dotnet_pwsh:3.1-runtime" }
        , @{ targetFramework = "net50"; image = "sqldatabase/dotnet_pwsh:5.0-runtime" }
        , @{ targetFramework = "net60"; image = "sqldatabase/dotnet_pwsh:6.0-runtime" }
    )

    $builds = @()
    foreach ($case in $testCases) {
        foreach ($database in $databases) {
            $builds += @{
                File            = "build-tasks.it-linux.ps1";
                Task            = "Test";
                settings        = $settings;
                targetFramework = $case.targetFramework;
                database        = $database;
                image           = $case.image;
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, targetFramework, image -MaximumBuilds 4
}

task NetRuntimeWindowsTest {
    $testCases = $(
        "net452"
        , "netcore31"
        , "net50"
        , "net60"
    )

    $builds = @()
    foreach ($case in $testCases) {
        foreach ($database in $databases) {
            $builds += @{
                File            = "build-tasks.it-win.ps1";
                Task            = "Test";
                settings        = $settings;
                targetFramework = $case;
                database        = $database;
            }
        }
    }

    Build-Parallel $builds -ShowParameter database, targetFramework -MaximumBuilds 4
}