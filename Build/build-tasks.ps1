#task Default Initialize, Clean, Build, ThirdPartyNotices, Pack, UnitTest, InitializeIntegrationTest, IntegrationTest
task Default Initialize, Clean, Build, ThirdPartyNotices, Pack, UnitTest, InitializeIntegrationTest, IntegrationTest
task Pack PackGlobalTool, PackPoweShellModule, PackNuget452, PackManualDownload

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
    Invoke-Build -File build-tasks.third-party.ps1 -Task "ThirdParty" -settings $settings
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

    # copy net452
    $net45Dest = Join-Path $dest "net452"
    $net45Source = Join-Path $settings.bin "SqlDatabase\net452"
    New-Item -Path $net45Dest -ItemType Directory  | Out-Null
    
    $files = @(
        "SqlDatabase.exe"
        , "SqlDatabase.pdb"
        , "System.Management.Automation.dll"
        , "Npgsql.dll"
        , "System.Threading.Tasks.Extensions.dll"
        , "System.Runtime.CompilerServices.Unsafe.dll"
        , "System.Memory.dll"
    )    
    foreach ($file in $files) {
        Copy-Item -Path (Join-Path $net45Source $file) -Destination $net45Dest
    }
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

    # netcoreapp2.2 build does not create .exe, copy it from netcoreapp3.1
    $exe = Join-Path $settings.bin "SqlDatabase\netcoreapp3.1\publish\SqlDatabase.exe"
    $destination = Join-Path $out "SqlDatabase.$packageVersion-netcore22.zip"
    $source = Join-Path $settings.bin "SqlDatabase\netcoreapp2.2\publish\*"
    Compress-Archive -Path $source, $exe, $lic, $thirdParty -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-netcore31.zip"
    $source = Join-Path $settings.bin "SqlDatabase\netcoreapp3.1\publish\*"
    Compress-Archive -Path $source, $lic, $thirdParty -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-net50.zip"
    $source = Join-Path $settings.bin "SqlDatabase\net5.0\publish\*"
    Compress-Archive -Path $source, $lic, $thirdParty -DestinationPath $destination
}

task UnitTest {
    $builds = @(
        @{ File = "build-tasks.unit-test.ps1"; Task = "Test"; settings = $settings; targetFramework = "net472" }
        @{ File = "build-tasks.unit-test.ps1"; Task = "Test"; settings = $settings; targetFramework = "netcoreapp2.2" }
        @{ File = "build-tasks.unit-test.ps1"; Task = "Test"; settings = $settings; targetFramework = "netcoreapp3.1" }
        @{ File = "build-tasks.unit-test.ps1"; Task = "Test"; settings = $settings; targetFramework = "net5.0" }
    )
    
    Build-Parallel $builds -MaximumBuilds 4
}

task InitializeIntegrationTest {
    $dest = $settings.integrationTests
    if (Test-Path $dest) {
        Remove-Item -Path $dest -Force -Recurse
    }

    Copy-Item -Path (Join-Path $settings.sources "SqlDatabase.Test\IntegrationTests") -Destination $dest -Force -Recurse
    Copy-Item -Path (Join-Path $settings.bin "Tests\net472\2.1_2.2.*") -Destination (Join-Path $dest "MsSql\Upgrade") -Force
    Copy-Item -Path (Join-Path $settings.bin "Tests\net472\2.1_2.2.*") -Destination (Join-Path $dest "PgSql\Upgrade") -Force

    # fix unix line endings
    $test = $dest + ":/test"
    exec {
        docker run --rm `
            -v $test `
            mcr.microsoft.com/dotnet/core/sdk:3.1 `
            bash -c "sed -i 's/\r//g' test/MsSql/TestGlobalTool.sh test/MsSql/Test.sh test/PgSql/TestGlobalTool.sh /test/PgSql/Test.sh"
    }
}

task IntegrationTest {
    $builds = @(
        # powershell desktop
        @{ File = "build-tasks.it-ps-desktop.ps1"; Task = "Test"; settings = $settings }
    )
    
    # powershell core
    $testCases = $(
        "mcr.microsoft.com/powershell:6.1.0-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:6.1.1-alpine-3.8"
        , "mcr.microsoft.com/powershell:6.1.2-alpine-3.8"
        , "mcr.microsoft.com/powershell:6.1.3-alpine-3.8"
        , "mcr.microsoft.com/powershell:6.2.0-alpine-3.8"
        , "mcr.microsoft.com/powershell:6.2.1-alpine-3.8"
        , "mcr.microsoft.com/powershell:6.2.4-alpine-3.8"
        , "mcr.microsoft.com/powershell:7.0.0-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.0.1-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.0.2-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.0.3-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.1.0-ubuntu-18.04"
        , "mcr.microsoft.com/powershell:7.1.2-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.2.0-preview.2-ubuntu-20.04"
        , "mcr.microsoft.com/powershell:7.2.0-preview.4-ubuntu-20.04")
    foreach ($case in $testCases) {
        $builds += @{ File = "build-tasks.it-ps-core.ps1"; Task = "Test"; settings = $settings; database = "MsSql"; image = $case }
        $builds += @{ File = "build-tasks.it-ps-core.ps1"; Task = "Test"; settings = $settings; database = "PgSql"; image = $case }
    }

    # sdk tool
    $testCases = $(
        "sqldatabase/dotnet_pwsh:2.2-sdk"
        , "sqldatabase/dotnet_pwsh:3.1-sdk"
        , "sqldatabase/dotnet_pwsh:5.0-sdk")
    foreach ($case in $testCases) {
        $builds += @{ File = "build-tasks.it-tool-linux.ps1"; Task = "Test"; settings = $settings; database = "MsSql"; image = $case }
        $builds += @{ File = "build-tasks.it-tool-linux.ps1"; Task = "Test"; settings = $settings; database = "PgSql"; image = $case }
    }

    # .net runtime linux
    $testCases = $(
        @{ targetFramework = "netcore22"; image = "sqldatabase/dotnet_pwsh:2.2-runtime" }
        , @{ targetFramework = "netcore31"; image = "sqldatabase/dotnet_pwsh:3.1-runtime" }
        , @{ targetFramework = "net50"; image = "sqldatabase/dotnet_pwsh:5.0-runtime" }
    )
    foreach ($case in $testCases) {
        $builds += @{ File = "build-tasks.it-linux.ps1"; Task = "Test"; settings = $settings; targetFramework = $case.targetFramework; database = "MsSql"; image = $case.image }
        $builds += @{ File = "build-tasks.it-linux.ps1"; Task = "Test"; settings = $settings; targetFramework = $case.targetFramework; database = "PgSql"; image = $case.image }
    }
    
    # .net runtime windows
    $testCases = $(
        "net452"
        , "netcore22"
        , "netcore31"
        , "net50"
    )
    foreach ($case in $testCases) {
        $builds += @{ File = "build-tasks.it-win.ps1"; Task = "Test"; settings = $settings; targetFramework = $case; database = "MsSql" }
        $builds += @{ File = "build-tasks.it-win.ps1"; Task = "Test"; settings = $settings; targetFramework = $case; database = "PgSql" }
    }

    Build-Parallel $builds -MaximumBuilds 4
}