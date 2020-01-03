using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotGit.Test.Utils;
using NUnit.Framework;

namespace DotGit.Test
{
    [SetUpFixture]
    public class RepositoryFixture
    {
        private static string TempRepoPath { get; } = Path.Combine(Path.GetTempPath(), "TempRepos");

        public static string SimpleRepoPath { get; } = Path.Combine(TempRepoPath, "SimpleRepo");

        [OneTimeSetUp]
        public void RestoreRepositories()
        {
            RepositoryUtils.ExtractZippedRepo("SimpleRepo", SimpleRepoPath);
        }

        [OneTimeTearDown]
        public void DeleteTempPath() => DeleteTempPathImpl();

        private static void DeleteTempPathImpl()
        {
            DirectoryUtils.Delete(TempRepoPath, true);
        }
    }
}
