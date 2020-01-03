using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DotGit.Models
{
    [DebuggerDisplay("Count = {Count}")]
    public class TreeCollection : Entry, IReadOnlyList<TreeEntry>
    {
        private readonly IImmutableList<Lazy<TreeEntry>> _entries;

        public TreeCollection(string hash, IImmutableList<Lazy<TreeEntry>> entries) : base(ObjectType.Tree, hash)
        {
            _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public TreeEntry this[int index] => _entries[index].Value;

        public int Count => _entries.Count;

        public IEnumerator<TreeEntry> GetEnumerator() => _entries.Select(e => e.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
