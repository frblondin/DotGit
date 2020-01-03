using System;
using System.Collections.Generic;
using System.Text;

namespace DotGit.Models
{
    public enum ObjectType
    {
        Commit = 1,
        Tree = 2,
        Blob = 3,
        Tag = 4,
        OfsDelta = 6,
        RefDelta = 7
    }
}
