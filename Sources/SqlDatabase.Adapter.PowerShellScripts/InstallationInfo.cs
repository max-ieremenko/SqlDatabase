namespace SqlDatabase.Adapter.PowerShellScripts;

[DebuggerDisplay("{Version}")]
internal readonly struct InstallationInfo : IEquatable<InstallationInfo>, IComparable<InstallationInfo>
{
    public InstallationInfo(string location, Version version, string? productVersion)
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

    public bool Equals(InstallationInfo other) =>
        StringComparer.InvariantCultureIgnoreCase.Equals(Location, other.Location)
        && Version == other.Version
        && StringComparer.InvariantCultureIgnoreCase.Equals(ProductVersion, other.ProductVersion);

    public override bool Equals(object? obj) => obj is InstallationInfo other && Equals(other);

    public override int GetHashCode()
    {
        var h1 = StringComparer.InvariantCultureIgnoreCase.GetHashCode(Location);
        var h2 = StringComparer.InvariantCultureIgnoreCase.GetHashCode(ProductVersion);
        var h3 = Version.GetHashCode();
        return h1 + h2 + h3;
    }

    private bool IsPreview() => ProductVersion.IndexOf("preview", StringComparison.OrdinalIgnoreCase) > 0;
}
