using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace DotGit
{
    internal class RepositoryLocker : IDisposable
    {
        private static readonly ConditionalWeakTable<RepositoryReader, object> _locks = new ConditionalWeakTable<RepositoryReader, object>();

        private readonly object _lock;
        private readonly RepositoryReader _reader;
        private readonly bool _reentrant;

        private bool _disposedValue = false; // To detect redundant calls

        internal RepositoryLocker(RepositoryReader reader)
        {
            _reader = reader;
            _lock = _locks.GetOrCreateValue(reader);
            _reentrant = LockDatabase();
        }

        private bool LockDatabase()
        {
            if (Monitor.IsEntered(_lock))
            {
                return true;
            }
            Monitor.Enter(_lock);

            var lockPath = GetLockFilePath();
            if (!SpinWait.SpinUntil(() => !File.Exists(lockPath), TimeSpan.FromSeconds(1)))
            {
                throw new NotSupportedException("Unable to create lock file. File already exists.");
            }
            using var file = File.Create(lockPath);
            File.SetAttributes(lockPath, FileAttributes.Hidden);
            return false;
        }

        private void UnlockDatabase()
        {
            if (!_reentrant)
            {
                File.Delete(GetLockFilePath());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                UnlockDatabase();
                _disposedValue = true;
            }
        }

         ~RepositoryLocker()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private string GetLockFilePath() => Path.Combine(_reader.Path, "index.lock");
    }
}
