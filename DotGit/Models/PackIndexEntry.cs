using System;
using System.Collections.Generic;
using System.Text;

namespace DotGit.Models
{
    public class PackIndexEntry
    {
        public PackIndexEntry(long offset, string objectName)
        {
            Offset = offset;
            ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
        }

        public long Offset { get; }
        public string ObjectName { get; }
    }
}
