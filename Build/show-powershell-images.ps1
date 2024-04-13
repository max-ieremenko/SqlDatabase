$ErrorActionPreference = 'Stop'

function Get-ShortVersion {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipeline)]
        [string]
        $FullVersion
    )
    
    process {
        # preview-7.5-ubuntu-20.04
        # 7.4-ubuntu-22.04
        # 7.3.0-preview.1-ubuntu-20.04
        $parts = $FullVersion -split '-'
        $version = $parts[0]
        $tag = $parts[1]

        if ($version -like 'preview*') {
            $version = $parts[1]
            $tag = $parts[0]
        }

        if ($tag -like 'preview*') {
            $version += '-' + $tag
        }

        return $version
    }
}

(Invoke-RestMethod -Uri 'https://mcr.microsoft.com/v2/powershell/tags/list').tags `
| Where-Object { ($_ -Like '[0-9]*') -or ($_ -Like 'preview-[0-9]*') } `
| Get-ShortVersion `
| Sort-Object -Unique