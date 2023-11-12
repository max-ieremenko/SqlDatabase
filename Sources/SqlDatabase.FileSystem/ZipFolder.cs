using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SqlDatabase.FileSystem;

[DebuggerDisplay("{Name}")]
internal sealed class ZipFolder : IFolder
{
    private readonly ZipFolder? _parent;
    private IFolder? _tree;

    public ZipFolder(string fileName)
        : this(null, fileName)
    {
    }

    public ZipFolder(ZipFolder? parent, string zipEntryFullName)
    {
        _parent = parent;

        Name = Path.GetFileName(zipEntryFullName);
        FileName = zipEntryFullName;
    }

    public string Name { get; }

    public string FileName { get; }

    public IEnumerable<IFolder> GetFolders() => BuildOrGetTree().GetFolders();

    public IEnumerable<IFile> GetFiles() => BuildOrGetTree().GetFiles();

    internal ZipArchive OpenRead()
    {
        if (_parent == null)
        {
            return ZipFile.OpenRead(FileName);
        }

        var file = new ZipFolderFile(_parent, _parent, FileName);
        return new ZipArchive(file.OpenRead(), ZipArchiveMode.Read, false);
    }

    private IFolder BuildOrGetTree()
    {
        if (_tree == null)
        {
            using (var zip = OpenRead())
            {
                _tree = BuildTree(zip.Entries);
            }
        }

        return _tree;
    }

    private IFolder BuildTree(IEnumerable<ZipArchiveEntry> entries)
    {
        /*
            1/
            1/11.txt
            2/
            2/2.txt
            2/2.2/2.2.txt
            inner.zip
         */

        var tree = new ZipEntryFolder(Name);

        foreach (var entry in entries)
        {
            var owner = tree;
            var path = entry.FullName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < path.Length - 1; i++)
            {
                var pathItem = path[i];
                if (!owner.FolderByName.TryGetValue(pathItem, out var next))
                {
                    next = new ZipEntryFolder(pathItem);
                    owner.FolderByName.Add(pathItem, next);
                }

                owner = (ZipEntryFolder)next;
            }

            var entryName = path.Last();
            if (entry.FullName.EndsWith("/"))
            {
                if (!owner.FolderByName.ContainsKey(entryName))
                {
                    owner.FolderByName.Add(entryName, new ZipEntryFolder(entryName));
                }
            }
            else if (FileTools.IsZip(entry.FullName))
            {
                owner.FolderByName.Add(entryName, new ZipFolder(this, entry.FullName));
            }
            else
            {
                owner.Files.Add(new ZipFolderFile(this, owner, entry.FullName));
            }
        }

        return tree;
    }
}