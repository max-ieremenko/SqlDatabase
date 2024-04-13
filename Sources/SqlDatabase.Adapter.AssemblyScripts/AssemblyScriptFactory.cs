using SqlDatabase.FileSystem;

namespace SqlDatabase.Adapter.AssemblyScripts;

public sealed class AssemblyScriptFactory : IScriptFactory, IScriptEnvironment
{
    private readonly FrameworkVersion _version;
    private readonly string? _configurationClassName;
    private readonly string? _configurationMethodName;

    public AssemblyScriptFactory(FrameworkVersion version, string? configurationClassName, string? configurationMethodName)
    {
        _version = version;
        _configurationClassName = configurationClassName;
        _configurationMethodName = configurationMethodName;
    }

    public bool IsSupported(IFile file) =>
        ".exe".Equals(file.Extension, StringComparison.OrdinalIgnoreCase)
        || ".dll".Equals(file.Extension, StringComparison.OrdinalIgnoreCase);

    public IScript FromFile(IFile file) => new AssemblyScript(
        _version,
        file.Name,
        _configurationClassName,
        _configurationMethodName,
        CreateBinaryReader(file),
        CreateScriptDescriptionReader(file));

    public bool IsSupported(IScript script) => script is AssemblyScript;

    public void Initialize(ILogger logger) => SubDomainFactory.Test(_version);

    private static Func<byte[]> CreateBinaryReader(IFile file) => () => BinaryRead(file);

    private static byte[] BinaryRead(IFile file)
    {
        using (var source = file.OpenRead())
        using (var dest = new MemoryStream())
        {
            source.CopyTo(dest);

            return dest.ToArray();
        }
    }

    private static Func<Stream?> CreateScriptDescriptionReader(IFile file)
    {
        return () =>
        {
            var parent = file.GetParent();
            if (parent == null)
            {
                return null;
            }

            var descriptionName = Path.GetFileNameWithoutExtension(file.Name) + ".txt";
            var description = parent.GetFiles().FirstOrDefault(i => string.Equals(descriptionName, i.Name, StringComparison.OrdinalIgnoreCase));

            return description?.OpenRead();
        };
    }
}