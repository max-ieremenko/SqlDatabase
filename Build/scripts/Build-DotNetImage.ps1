function Build-DotNetImage {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)]
        [ValidateSet('sdk', 'runtime')]
        [string]
        $Type,

        [Parameter(Mandatory)]
        [string]
        $DotNetVersion,

        [Parameter(Mandatory)]
        [string]
        $PwshVersion
    )

    $dockerfile = Join-Path $PSScriptRoot '../../Sources/Docker/image-dotnet.dockerfile'
    $tag = "sqldatabase/dotnet_pwsh:$DotNetVersion-$Type"

    exec {
        docker build `
            --pull `
            --file $dockerfile `
            --build-arg "BASE_IMAGE=mcr.microsoft.com/dotnet/$($Type):$DotNetVersion-noble-amd64" `
            --build-arg "PWSH_VERSION=$PwshVersion" `
            --tag $tag `
            .
    }

    exec { docker run -it --rm $tag dotnet --list-sdks }
    exec { docker run -it --rm $tag dotnet --list-runtimes }
    exec { docker run -it --rm $tag pwsh --version }
}