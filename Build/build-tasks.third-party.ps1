param(
    $settings
)

task Default Update, Test, Publish

Enter-Build {
    $repository = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "third-party-libraries"))

    $sourceDir = $settings.sources
    $sources = (Join-Path $sourceDir "SqlDatabase"),
        (Join-Path $sourceDir "SqlDatabase.Test"),
        (Join-Path $sourceDir "SqlDatabase.PowerShell"),
        (Join-Path $sourceDir "SqlDatabase.PowerShell.Test"),
        (Join-Path $sourceDir "SqlDatabase.Test")
}

task Update {
    Update-ThirdPartyLibrariesRepository -AppName "SqlDatabase" -Source $sources -Repository $repository
}

task Test {
    Test-ThirdPartyLibrariesRepository -AppName "SqlDatabase" -Source $sources -Repository $repository
}

task Publish {
    $outTemp = Join-Path $settings.bin "Temp"
    $title = "SqlDatabase " + $settings.version
    Publish-ThirdPartyNotices -AppName "SqlDatabase" -Repository $repository -Title $title -To $outTemp

    Move-Item (Join-Path $outTemp "ThirdPartyNotices.txt") $settings.bin -Force
    Remove-Item -Path $outTemp -Recurse -Force
}