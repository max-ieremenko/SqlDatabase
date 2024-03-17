namespace SqlDatabase.Adapter.PowerShellScripts;

[DebuggerDisplay("{Version}")]
internal readonly struct InstallationInfo : IComparable<InstallationInfo>
{
    public InstallationInfo(string location, Version version, string productVersion)
    {
        Location = location;
        Version = version;
        ProductVersion = productVersion ?? string.Empty;
    }

    public string Location { get; }

    public Version Version { get; }

    public string ProductVersion { get; }

    public int CompareTo(InstallationInfo other)
    {
        var result = Version.CompareTo(other.Version);
        if (result != 0)
        {
            return result;
        }

        var isPreview = IsPreview();
        var otherIsPreview = other.IsPreview();
        if (isPreview && !otherIsPreview)
        {
            return -1;
        }

        if (!isPreview && otherIsPreview)
        {
            return 1;
        }

        result = StringComparer.InvariantCultureIgnoreCase.Compare(ProductVersion, other.ProductVersion);
        if (result == 0)
        {
            result = StringComparer.InvariantCultureIgnoreCase.Compare(Location, other.Location);
        }

        return result;
    }

    private bool IsPreview()
    {
        return ProductVersion.IndexOf("preview", StringComparison.OrdinalIgnoreCase) > 0;
    }
}
