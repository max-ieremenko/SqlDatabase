param(
    $settings
)

function Write-ThirdPartyNotices($appName, $sources, $repository, $out) {
    $source = $sources | ForEach-Object {"-source", $_}
    $outTemp = Join-Path $out "Temp"

    Exec {
        ThirdPartyLibraries update `
            -appName $appName `
            $source `
            -repository $repository
    }

    Exec {
        ThirdPartyLibraries validate `
            -appName $appName `
            $source `
            -repository $repository
    }

    Exec {
        ThirdPartyLibraries generate `
            -appName $appName `
            -repository $repository `
            -to $outTemp
    }

    Move-Item (Join-Path $outTemp "ThirdPartyNotices.txt") $out -Force
    Remove-Item -Path $outTemp -Recurse -Force
}

task ThirdParty {
    $sourceDir = $settings.sources
    $thirdPartyRepository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "third-party-libraries"))
    $binDir = $settings.bin

    $sources = @(
        (Join-Path $sourceDir "SqlDatabase"),
        (Join-Path $sourceDir "SqlDatabase.Test"),
        (Join-Path $sourceDir "SqlDatabase.PowerShell"),
        (Join-Path $sourceDir "SqlDatabase.PowerShell.Test"),
        (Join-Path $sourceDir "SqlDatabase.Test")
    )
    
    Write-ThirdPartyNotices "SqlDatabase" $sources $thirdPartyRepository $binDir
}