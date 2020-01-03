using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DotGit.Models
{
    [DebuggerDisplay("{Hash}")]
    public class Entry
    {
        public Entry(ObjectType type, string hash)
        {
            Type = type;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public ObjectType Type { get; }
        public string Hash { get; }
    }
}
