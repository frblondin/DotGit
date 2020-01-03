using DotGit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DotGit
{
    public class RepositoryReader
    {
        public RepositoryReader(string path)
        {
            Path = FixRepository(path ?? throw new ArgumentNullException(nameof(path)));
        }

        public string Path { get; }

        public T Read<T>(string hash)
           where T : Entry
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            using (new RepositoryLocker(this))
            {
                return ObjectFileReader.Read<T>(this, hash) ??
                    PackReader.Read<T>(this, Path, hash);
            }
        }

        private static string FixRepository(string repository)
        {
            var path = System.IO.Path.Combine(repository, ".git");
            if (!Directory.Exists(path))
            {
                throw new DotGitException("No git database folder could be found.");
            }
            return path;
        }
    }
}
