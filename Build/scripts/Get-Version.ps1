function Get-Version {
    param (
        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ })]
        [string]
        $SourcePath
    )

    $buildProps = Join-Path $SourcePath 'Directory.Build.props'
    $xml = Select-Xml -Path $buildProps -XPath 'Project/PropertyGroup/SqlDatabaseVersion'

    $xml.Node.InnerText
}