using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotGit.IO
{
    public class LockingStream : Stream
    {
        private readonly RepositoryLocker _locker;
        private readonly RepositoryReader _reader;

        public LockingStream(RepositoryReader reader, Stream wrapped)
        {
            _locker = new RepositoryLocker(reader);
            _reader = reader;
            Wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
        }

        public Stream Wrapped { get; }

        public override bool CanRead => Wrapped.CanRead;

        public override bool CanSeek => Wrapped.CanSeek;

        public override bool CanWrite => Wrapped.CanWrite;

        public override long Length => Wrapped.Length;

        public override long Position
        {
            get => Wrapped.Position;
            set => Wrapped.Position = value;
        }

        public override void Flush() => Wrapped.Flush();

        public override int Read(byte[] buffer, int offset, int count) => Wrapped.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => Wrapped.Seek(offset, origin);

        public override void SetLength(long value) => Wrapped.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => Wrapped.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            _locker.Dispose();
            Wrapped.Dispose();
            base.Dispose(disposing);
        }
    }
}
