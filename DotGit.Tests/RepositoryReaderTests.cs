using DotGit.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGit.Test
{
    [TestFixture]
    public class RepositoryReaderTests
    {
        [Test]
        public void TestReadsCommitFromPack()
        {
            // Arrange
            var reader = new RepositoryReader(RepositoryFixture.SimpleRepoPath);

            // Act
            var commit = reader.Read<Commit>("c462e3f3024c8cabc252ed5d309922ed06a492b9");

            // Assert
            Assert.That(commit, Is.Not.Null);
            Assert.That(commit.Hash, Is.EqualTo("c462e3f3024c8cabc252ed5d309922ed06a492b9"));
            Assert.That(commit.Author.Name, Is.EqualTo("Roger Rabbit"));
            Assert.That(commit.Author.Email, Is.EqualTo("roger.rabbit@acme.com"));
            Assert.That(commit.Committer.Name, Is.EqualTo("Roger Rabbit"));
            Assert.That(commit.Message, Is.EqualTo("New\none"));
            Assert.That(commit.Parents, Has.Exactly(1).Items);
            Assert.That(commit.Parents[0].Hash, Is.EqualTo("3d9640ea3da190c5c1dabdcc9c16515924ec009a"));
            Assert.That(commit.Tree.Hash, Is.EqualTo("622196575751a798e7f6f74d553a506590af3d91"));
            Assert.That(commit.Type, Is.EqualTo(ObjectType.Commit));
        }

        [Test]
        public void TestReadsTreeFromPack()
        {
            // Arrange
            var reader = new RepositoryReader(RepositoryFixture.SimpleRepoPath);

            // Act
            var tree = reader.Read<TreeCollection>("622196575751a798e7f6f74d553a506590af3d91");

            // Assert
            Assert.That(tree, Is.Not.Null);
            Assert.That(tree.Hash, Is.EqualTo("622196575751a798e7f6f74d553a506590af3d91"));
            Assert.That(tree.Count, Is.EqualTo(3));
            Assert.That(tree[0].Hash, Is.EqualTo("95a69660cfc8d017e79147bbf0b350210e3e0963"));
            Assert.That(Traverse(tree).Count(), Is.EqualTo(4));
        }

        [Test]
        public void TestReadsBlobFromPack()
        {
            // Arrange
            var reader = new RepositoryReader(RepositoryFixture.SimpleRepoPath);

            // Act
            var blob = reader.Read<Blob>("0a207c060e61f3b88eaee0a8cd0696f46fb155eb");

            // Assert
            Assert.That(blob, Is.Not.Null);
            Assert.That(blob.Hash, Is.EqualTo("0a207c060e61f3b88eaee0a8cd0696f46fb155eb"));
            Assert.That(blob.ReadAsString(), Is.EqualTo("a\nb"));
        }

        [Test]
        public void TestReadsCommitFromObjectFile()
        {
            // Arrange
            var reader = new RepositoryReader(RepositoryFixture.SimpleRepoPath);

            // Act
            var commit = reader.Read<Commit>("9c257ee74e77fbbcce53b05f3bbea255951625c8");

            // Assert
            Assert.That(commit, Is.Not.Null);
            Assert.That(commit.Hash, Is.EqualTo("9c257ee74e77fbbcce53b05f3bbea255951625c8"));
            Assert.That(commit.Author.Name, Is.EqualTo("Roger Rabbit"));
            Assert.That(commit.Author.Email, Is.EqualTo("roger.rabbit@acme.com"));
            Assert.That(commit.Message, Is.EqualTo("New addition"));
            Assert.That(commit.Parents, Has.Exactly(1).Items);
            Assert.That(commit.Parents[0].Hash, Is.EqualTo("8f17fc4eeb796c9fd9a30b82be8636f1f1ac6a6f"));
            Assert.That(commit.Tree.Hash, Is.EqualTo("48b544940332d0d19bd31a45957648049f021788"));
            Assert.That(commit.Type, Is.EqualTo(ObjectType.Commit));
        }

        [Test]
        public void TestReadsTreeFromObjectFile()
        {
            // Arrange
            var reader = new RepositoryReader(RepositoryFixture.SimpleRepoPath);

            // Act
            var tree = reader.Read<TreeCollection>("48b544940332d0d19bd31a45957648049f021788");

            // Assert
            Assert.That(tree, Is.Not.Null);
            Assert.That(tree.Hash, Is.EqualTo("48b544940332d0d19bd31a45957648049f021788"));
            Assert.That(tree.Count, Is.EqualTo(5));
            Assert.That(tree[0].Hash, Is.EqualTo("95a69660cfc8d017e79147bbf0b350210e3e0963"));
        }

        private static IEnumerable<TreeEntry> Traverse(TreeCollection collection)
        {
            var stack = new Stack<TreeEntry>();
            PushChildren(collection);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                if (current.Entry is TreeCollection children)
                {
                    PushChildren(children);
                }
            }

            void PushChildren(TreeCollection entries)
            {
                foreach (var child in entries)
                {
                    stack.Push(child);
                }
            }
        }
    }
}