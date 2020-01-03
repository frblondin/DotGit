using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DotGit.Models
{
    [DebuggerDisplay("Name = {Name}, Hash = {Hash}")]
    public class TreeEntry : Entry
    {
        public TreeEntry(string name, Entry entry, int mode) : base(ObjectType.Tree, entry?.Hash)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            Mode = mode;
        }

        public string Name { get; }
        public Entry Entry { get; }
        public int Mode { get; }
    }
}
