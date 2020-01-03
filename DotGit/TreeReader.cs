using DotGit.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGit
{
    internal static class TreeReader
    {
        internal static TreeCollection Read(RepositoryReader reader, string hash, Stream stream, long length)
        {
            var entries = ImmutableList.CreateBuilder<Lazy<TreeEntry>>();
            while (length > 0)
            {
                var mode = stream.ReadUpTo(' ');
                var name = stream.ReadUpTo('\0');
                length -= mode.Length + 1 + name.Length + 1;
                var buffer = new byte[20];
                stream.Read(buffer, 0, 20);
                var childHash = string.Concat(buffer.Select(x => x.ToString("x2")));
                length -= 20;
                entries.Add(new Lazy<TreeEntry>(() =>
                {
                    var entry = reader.Read<Entry>(childHash);
                    return new TreeEntry(name, entry, int.Parse(mode, CultureInfo.InvariantCulture));
                }));
            }
            return new TreeCollection(hash, entries.ToImmutable());
        }
    }
}
