using System;
using System.Collections.Generic;
using System.Text;

namespace DotGit.Tools
{
    internal static class StringUtils
    {
        internal static string ReadSingleValue(string line, string prefix, bool throwIfMissing)
        {
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!throwIfMissing)
                {
                    return null;
                }
                throw new DotGitException("Invalid object.");
            }
            return line.Substring(prefix.Length);
        }
    }
}
