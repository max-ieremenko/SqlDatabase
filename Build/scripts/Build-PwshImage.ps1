function Build-PwshImage {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)]
        [string]
        $OsVersion,

        [Parameter(Mandatory)]
        [string]
        $PwshVersion
    )

    $dockerfile = Join-Path $PSScriptRoot '../../Sources/Docker/image-pwsh.dockerfile'
    $tag = "sqldatabase/pwsh:$PwshVersion"

    $package = 'powershell'
    $alias = 'poweshell'
    $aliasCommand = 'pwsh'
    if ($PwshVersion.Contains('-preview') -or $PwshVersion.Contains('-rc')) {
        $package = 'powershell-preview'
        $alias = 'pwsh'
        $aliasCommand = 'pwsh-preview'
    }

    exec {
        docker build `
            --pull `
            --file $dockerfile `
            --build-arg "OS_VERSION=$OsVersion" `
            --build-arg "PWSH_VERSION=$PwshVersion" `
            --build-arg "PWSH_PACKAGE=$package" `
            --build-arg "PWSH_ALIAS=$alias" `
            --build-arg "PWSH_ALIAS_COMMAND=$aliasCommand" `
            --tag $tag `
            .
    }

    exec { docker run -it --rm $tag pwsh --version }
}