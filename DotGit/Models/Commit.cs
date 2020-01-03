using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace DotGit.Models
{
    [DebuggerDisplay("Message = {Message}, Hash = {Hash}")]
    public class Commit : Entry
    {
        private readonly Lazy<TreeCollection> _tree;
        private readonly Lazy<IImmutableList<Commit>> _parents;

        public Commit(string hash, Lazy<TreeCollection> tree, Lazy<IImmutableList<Commit>> parents, Signature author, Signature committer, string message)
            : base(ObjectType.Commit, hash)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            _parents = parents ?? throw new ArgumentNullException(nameof(parents));
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Committer = committer ?? throw new ArgumentNullException(nameof(committer));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public TreeCollection Tree => _tree.Value;
        public IImmutableList<Commit> Parents => _parents.Value;
        public Signature Author { get; }
        public Signature Committer { get; }
        public string Message { get; }
    }
}
