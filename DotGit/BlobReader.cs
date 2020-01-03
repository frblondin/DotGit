using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotGit.Models;

namespace DotGit
{
    internal static class BlobReader
    {
        internal static Blob Read(RepositoryReader reader, string hash, Stream stream, Func<Stream> streamProvider)
        {
            var position = stream.Position;
            return new Blob(reader, hash, () =>
            {
                var s = streamProvider();
                s.Seek(position, SeekOrigin.Begin);
                return s.DeflateForZlibData();
            });
        }
    }
}
