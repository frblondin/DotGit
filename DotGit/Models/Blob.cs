using DotGit.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotGit.Models
{
    public class Blob : Entry
    {
        private readonly RepositoryReader _reader;
        internal readonly Func<Stream> _streamProvider;

        public Blob(RepositoryReader reader, string hash, Func<Stream> streamProvider)
            : base(ObjectType.Blob, hash)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _streamProvider = streamProvider ?? throw new ArgumentNullException(nameof(streamProvider));
        }

        public Stream Stream => new LockingStream(_reader, _streamProvider());

        public string ReadAsString()
        {
            using (var reader = new StreamReader(Stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
