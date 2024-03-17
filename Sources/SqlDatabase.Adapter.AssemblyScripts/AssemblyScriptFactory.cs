using SqlDatabase.FileSystem;

namespace SqlDatabase.Adapter.AssemblyScripts;

public sealed class AssemblyScriptFactory : IScriptFactory, IScriptEnvironment
{
    private readonly string? _configurationClassName;
    private readonly string? _configurationMethodName;

    public AssemblyScriptFactory(string? configurationClassName, string? configurationMethodName)
    {
        _configurationClassName = configurationClassName;
        _configurationMethodName = configurationMethodName;
    }

    public bool IsSupported(IFile file)
    {
        return ".exe".Equals(file.Extension, StringComparison.OrdinalIgnoreCase)
               || ".dll".Equals(file.Extension, StringComparison.OrdinalIgnoreCase);
    }

    public IScript FromFile(IFile file)
    {
        return new AssemblyScript(
            file.Name,
            _configurationClassName,
            _configurationMethodName,
            CreateBinaryReader(file),
            CreateScriptDescriptionReader(file));
    }

    public bool IsSupported(IScript script) => script is AssemblyScript;

    public void Initialize(ILogger logger) => SubDomainFactory.Test();

    private static Func<byte[]> CreateBinaryReader(IFile file)
    {
        return () => BinaryRead(file);
    }

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