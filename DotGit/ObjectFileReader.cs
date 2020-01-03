using DotGit.IO;
using DotGit.Models;
using DotGit.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DotGit
{
    internal static class ObjectFileReader
    {
        public static T Read<T>(RepositoryReader reader, string hash)
            where T : Entry
        {
            var path = Path.Combine(reader.Path, "objects", hash.Substring(0, 2), hash.Substring(2));
            if (File.Exists(path))
            {
                return (T)Read(reader, hash, path);
            }
            return null;
        }

        private static Entry Read(RepositoryReader reader, string hash, string path)
        {
            using (var stream = File.OpenRead(path).DeflateForZlibData())
            {
                var (type, length) = ReadHeader(stream);
                switch (type)
                {
                    case "commit":
                        return CommitReader.Read(reader, hash, stream);
                    case "tree":
                        return TreeReader.Read(reader, hash, stream, length);
                    case "blob":
                        return new Blob(reader, hash, () =>
                        {
                            var s = File.OpenRead(path).DeflateForZlibData();
                            ReadHeader(s);
                            return s;
                        });
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static (string type, int size) ReadHeader(Stream stream)
        {
            var type = stream.ReadUpTo(' ');
            var size = stream.ReadUpTo('\0');
            return (type, int.Parse(size, CultureInfo.InvariantCulture));
        }
    }
}
