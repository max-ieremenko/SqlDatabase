Include ".\build-scripts.ps1"

Task default -Depends Initialize, Clean, Build, Pack, UnitTest, Test
Task Pack -Depends PackGlobalTool, PackNet452, PackChoco, PackManualDownload
Task UnitTest -Depends InitializeTests `
    , UnitTest452 `
    , UnitTest472 `
    , UnitTestcore22 `
    , UnitTestcore31 `
    , UnitTest50
Task Test -Depends InitializeTests `
    , TestPublishModule `
    , TestPowerShellDesktop `
    , TestPowerShellCore610 `
    , TestPowerShellCore611 `
    , TestPowerShellCore612 `
    , TestPowerShellCore613 `
    , TestPowerShellCore620 `
    , TestPowerShellCore621 `
    , TestPowerShellCore624 `
    , TestPowerShellCore70 `
    , TestPowerShellCore701 `
    , TestPowerShellCore702 `
    , TestPowerShellCore703 `
    , TestPowerShellCore710 `
    , TestPowerShellCore720 `
    , TestGlobalTool22 `
    , TestGlobalTool31 `
    , TestGlobalTool50 `
    , TestNetCore22 `
    , TestNetCore31 `
    , TestNet50

Task Initialize {
    $script:nugetexe = Join-Path $PSScriptRoot "nuget.exe"
    $script:sourceDir = Join-Path $PSScriptRoot "..\Sources"
    $script:binDir = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\bin"))
    $script:binNugetDir = Join-Path $binDir "nuget"
    $script:binChocoDir = Join-Path $binDir "choco"
    $script:packageVersion = Get-AssemblyVersion (Join-Path $sourceDir "GlobalAssemblyInfo.cs")
    $script:repositoryCommitId = Get-RepositoryCommitId

    $script:moduleBin = Join-Path $binDir "SqlDatabase.PowerShell\netstandard2.0\"
    $script:moduleIntegrationTests = Join-Path $binDir "IntegrationTests"

    $mssql = Resolve-SqlServerIp "mssql"
    $script:connectionString = "Data Source=$mssql;Initial Catalog=SqlDatabaseTest;User Id=sa;Password=P@ssw0rd;"

    Write-Host "PackageVersion: $packageVersion"
    Write-Host "CommitId: $repositoryCommitId"
}

Task Clean {
    if (Test-Path $binDir) {
        Remove-Item -Path $binDir -Recurse -Force
    }

    New-Item -Path $binDir -ItemType Directory | Out-Null
    New-Item -Path $binNugetDir -ItemType Directory | Out-Null
    New-Item -Path $binChocoDir -ItemType Directory | Out-Null
}

Task Build {
    $solutionFile = Join-Path $sourceDir "SqlDatabase.sln"
    Exec { dotnet restore $solutionFile }
    Exec { dotnet build $solutionFile -t:Rebuild -p:Configuration=Release }

    # .psd1 set module version
    $psdFiles = Get-ChildItem -Path $binDir -Filter "SqlDatabase.psd1" -Recurse
    foreach ($psdFile in $psdFiles) {
        ((Get-Content -Path $psdFile.FullName -Raw) -replace '{{ModuleVersion}}', $packageVersion) | Set-Content -Path $psdFile.FullName
    }

    # copy to powershell
    Copy-Item -Path (Join-Path $sourceDir "..\LICENSE.md") -Destination $moduleBin
    $net45Dest = Join-Path $moduleBin "net452"
    $net45Source = Join-Path $binDir "SqlDatabase\net452"
    New-Item -Path $net45Dest -ItemType Directory
    Copy-Item -Path (Join-Path $net45Source "SqlDatabase.exe") -Destination $net45Dest
    Copy-Item -Path (Join-Path $net45Source "SqlDatabase.pdb") -Destination $net45Dest
}

Task PackGlobalTool {
    $projectFile = Join-Path $sourceDir "SqlDatabase\SqlDatabase.csproj"

    Exec {
        dotnet pack `
            -c Release `
            -p:PackAsTool=true `
            -p:GlobalTool=true `
            -p:PackageVersion=$packageVersion `
            -p:RepositoryCommit=$repositoryCommitId `
            -o $binNugetDir `
            $projectFile
    }
}

Task PackNet452 {
    $bin = $moduleBin
    if (-not $bin.EndsWith("\")) {
        $bin += "\"
    }

    $nuspec = Join-Path $sourceDir "SqlDatabase.Package\nuget\package.nuspec"
    Exec { 
        & $nugetexe pack `
            -NoPackageAnalysis `
            -verbosity detailed `
            -OutputDirectory $binNugetDir `
            -Version $packageVersion `
            -p RepositoryCommit=$repositoryCommitId `
            -p bin=$bin `
            $nuspec
    }
}

Task PackChoco {
    $bin = $moduleBin
    if (-not $bin.EndsWith("\")) {
        $bin += "\"
    }

    $nuspec = Join-Path $sourceDir "SqlDatabase.Package\choco\sqldatabase.nuspec"
    Exec { 
        choco pack `
            $nuspec `
            --outputdirectory $binChocoDir `
            --version $packageVersion `
            -p bin=$bin
    }
}

Task PackManualDownload {
    $out = Join-Path $binDir "ManualDownload"
    New-Item -Path $out -ItemType Directory | Out-Null

    $lic = Join-Path $sourceDir "..\LICENSE.md"
    
    $destination = Join-Path $out "SqlDatabase.$packageVersion-net452.zip"
    $source = Join-Path $binDir "SqlDatabase\net452\*"
    Compress-Archive -Path $source, $lic -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-PowerShell.zip"
    $source = Join-Path $moduleBin "*"
    Compress-Archive -Path $source -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-netcore22.zip"
    $source = Join-Path $binDir "SqlDatabase\netcoreapp2.2\publish\*"
    Compress-Archive -Path $source, $lic -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-netcore31.zip"
    $source = Join-Path $binDir "SqlDatabase\netcoreapp3.1\publish\*"
    Compress-Archive -Path $source, $lic -DestinationPath $destination

    $destination = Join-Path $out "SqlDatabase.$packageVersion-net50.zip"
    $source = Join-Path $binDir "SqlDatabase\net5.0\publish\*"
    Compress-Archive -Path $source, $lic -DestinationPath $destination
}

Task InitializeTests {
    Copy-Item -Path (Join-Path $sourceDir "SqlDatabase.Test\IntegrationTests") -Destination $binDir -Force -Recurse
    Copy-Item -Path (Join-Path $binDir "Tests\net452\2.1_2.2.*") -Destination (Join-Path $binDir "IntegrationTests\Upgrade") -Force -Recurse

    # fix unix line endings
    $test = $moduleIntegrationTests + ":/test"
    Exec {
        docker run --rm `
            -v $test `
            mcr.microsoft.com/dotnet/core/sdk:3.1 `
            bash -c "sed -i 's/\r//g' /test/TestGlobalTool.sh /test/Test.sh"
    }
}

Task UnitTest452 {
    Test-Unit "net452"
}

Task UnitTest472 {
    Test-Unit "net472"
}

Task UnitTestcore22 {
    Test-Unit "netcoreapp2.2"
}

Task UnitTestcore31 {
    Test-Unit "netcoreapp3.1"
}

Task UnitTest50 {
    Test-Unit "net5.0"
}

Task TestPowerShellCore611 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:6.1.1-alpine-3.8"
}

Task TestPowerShellCore610 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:6.1.0-ubuntu-18.04"
}

Task TestPowerShellCore612 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:6.1.2-alpine-3.8"
}

Task TestPowerShellCore613 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:6.1.3-alpine-3.8"
}

Task TestPowerShellCore620 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:6.2.0-alpine-3.8"
}

Task TestPowerShellCore621 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:6.2.1-alpine-3.8"
}

Task TestPowerShellCore624 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:6.2.4-alpine-3.8"
}

Task TestPowerShellCore70 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:7.0.0-ubuntu-18.04"
}

Task TestPowerShellCore701 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:7.0.1-ubuntu-18.04"
}

Task TestPowerShellCore702 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:7.0.2-ubuntu-18.04"
}

Task TestPowerShellCore703 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:7.0.3-ubuntu-18.04"
}

Task TestPowerShellCore710 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:7.1.0-ubuntu-18.04"
}

Task TestPowerShellCore720 {
    Test-PowerShellCore "mcr.microsoft.com/powershell:7.2.0-preview.1-ubuntu-20.04"
}

Task TestPublishModule {
    $log = Join-Path $binDir "Publish-Module.whatif.log"

    Test-PowerShellDesktop "Publish-Module -Name SqlDatabase -WhatIf -Verbose -NuGetApiKey 123 *> $log"
}

Task TestPowerShellDesktop {
    $env:test = $moduleIntegrationTests

    $builder = New-Object -TypeName System.Data.SqlClient.SqlConnectionStringBuilder -ArgumentList $connectionString
    $builder["Data Source"] = "."
    $env:connectionString = $builder.ToString()

    $testScript = Join-Path $moduleIntegrationTests "Test.ps1"

    Test-PowerShellDesktop ". $testScript"
}

Task TestGlobalTool22 {
    Test-GlobalTool "microsoft/dotnet:2.2-sdk"
}

Task TestGlobalTool31 {
    Test-GlobalTool "mcr.microsoft.com/dotnet/core/sdk:3.1"
}

Task TestGlobalTool50 {
    Test-GlobalTool "mcr.microsoft.com/dotnet/sdk:5.0"
}

Task TestNetCore22 {
    Test-NetCore "netcoreapp2.2" "microsoft/dotnet:2.2-runtime"
}

Task TestNetCore31 {
    Test-NetCore "netcoreapp3.1" "mcr.microsoft.com/dotnet/core/runtime:3.1"
}

Task TestNet50 {
    Test-NetCore "net5.0" "mcr.microsoft.com/dotnet/runtime:5.0"
}
