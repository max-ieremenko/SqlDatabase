using System.IO;
using System.Text;

namespace SqlDatabase.IO
{
    internal sealed class InLineScriptFile : IFile
    {
        private readonly string _content;

        public InLineScriptFile(string name, string content)
        {
            _content = content;
            Name = name;
        }

        public string Name { get; }

        public Stream OpenRead()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_content));
        }
    }
}
