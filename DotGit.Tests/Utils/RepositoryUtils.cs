using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DotGit.Test.Utils
{
    internal class RepositoryUtils
    {
        internal static string ExtractZippedRepo(string repoName, string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            using (var resourceStream = typeof(RepositoryUtils).Assembly.GetManifestResourceStream($"DotGit.Tests.Repos.{repoName}.zip"))
            using (var archive = new ZipArchive(resourceStream))
            {
                archive.ExtractToDirectory(path);
            }
            return path;
        }
    }
}