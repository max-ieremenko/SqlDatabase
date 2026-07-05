#Requires -Version "7.3"

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

function Get-Version {
    param (
        [Parameter(Mandatory)]
        [string]
        $Tag
    )

    # do not relay on "prerelease" flag from response: v6.1.0-preview.3 "prerelease": false
    # v7.3.0 => 20_000
    # v7.3.0-rc.1 => 15_001
    # v7.3.0-preview.7 => 10_007
    # exceptions: v6.0.0-rc
    $version = $Tag.Substring(1)
    $comparer = 20000

    $parts = $version.Split('-')
    if ($parts.Count -gt 1) {
        $version = $parts[0]

        $parts = $parts[1].Split('.')
        $comparer = ($parts[0] -eq 'rc') ? 15000 : 10000
        $comparer += $parts.Count -gt 1 ? [int]$parts[1] : 0
    }

    @{ 
        Version  = [version]$version
        Comparer = $comparer
    }
}

function Get-Releases {
    param (
        [Parameter(Mandatory)]
        [Version]
        $VersionFrom
    )

    $processed = New-Object System.Collections.Generic.Dictionary[[version]`,[object]]
    $pageNum = 1
    $changed = $true
    $versionFromFound = $false

    while ($changed -and (-not $versionFromFound)) {
        $page = Invoke-RestMethod -Uri "https://api.github.com/repos/powershell/powershell/releases?per_page=100&page=$pageNum"
        $changed = $false
        $pageNum++

        foreach ($item in $page) {
            if ($item.draft) {
                continue
            }

            $version = Get-Version -Tag $item.tag_name
            if ($version.Version -lt $VersionFrom) {
                continue
            }
    
            $release = [PSCustomObject]@{
                Source   = $item
                Version  = $version.Version
                Comparer = $version.Comparer
            }

            if ($processed.ContainsKey($version.Version)) {
                $existing = $processed[$version.Version]
                if ($version.Comparer -le $existing.Comparer) {
                    continue
                }
            }

            $processed[$version.Version] = $release
            $changed = $true
            if ($version.Version -eq $VersionFrom) {
                $versionFromFound = $true
            }
        }
    }

    foreach ($item in $processed.Values) {
        [PSCustomObject]@{
            Tag          = $item.Source.tag_name
            Version      = $item.Version
            MajorVersion = New-Object version -ArgumentList $item.Version.Major, $item.Version.Minor
            PublishedAt  = [datetime]$item.Source.published_at
            Url          = $item.Source.html_url
        }
    }
}

# https://learn.microsoft.com/en-us/powershell/scripting/install/powershell-support-lifecycle
$table = Get-Releases  -VersionFrom '7.2.0' | `
    Group-Object -Property MajorVersion | `
    Sort-Object -Property Name -Descending | `
    ForEach-Object -Process {
    $versions = $_.Group | Sort-Object -Property Version
    if ($versions.Count -gt 1) {
        $versions[$versions.Count - 1]
    }

    $versions[0]
}

$table | Format-Table -AutoSize -RepeatHeader `
    -Property Tag, @{ Label = 'PublishedAt'; Expression = { $_.PublishedAt.ToLocalTime().ToString('F') } }, Url
