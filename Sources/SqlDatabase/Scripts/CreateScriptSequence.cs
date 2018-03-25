using System.Collections.Generic;
using System.Linq;
using SqlDatabase.IO;

namespace SqlDatabase.Scripts
{
    internal sealed class CreateScriptSequence : ICreateScriptSequence
    {
        public IFolder Root { get; set; }

        public IScriptFactory ScriptFactory { get; set; }

        public IList<IScript> BuildSequence()
        {
            var result = new List<IScript>();
            Build(Root, ScriptFactory, result);

            return result;
        }

        private static void Build(IFolder root, IScriptFactory factory, List<IScript> scripts)
        {
            var files = root
                .GetFiles()
                .Where(i => factory.IsSupported(i.Name))
                .OrderBy(i => i.Name)
                .Select(factory.FromFile);
            scripts.AddRange(files);

            var folders = root
                .GetFolders()
                .OrderBy(i => i.Name);
            foreach (var subFolder in folders)
            {
                Build(subFolder, factory, scripts);
            }
        }
    }
}
