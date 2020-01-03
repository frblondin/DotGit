using System;
using System.Collections.Generic;
using System.Text;

namespace DotGit.Models
{
    public class Pack
    {
        public Pack(int version, int objectCount, int entryCount)
        {
            Version = version;
            ObjectCount = objectCount;
            EntryCount = entryCount;
        }

        public int Version { get; }
        public int ObjectCount { get; }
        public int EntryCount { get; }
    }
}
