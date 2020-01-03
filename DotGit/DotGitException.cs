using System;
using System.Collections.Generic;
using System.Text;

namespace DotGit
{
    /// <summary>
    /// Represents errors that occur during DotGit execution.
    /// </summary>
    public class DotGitException : Exception
    {
        public DotGitException(string message, Exception innerException = null) : base(message, innerException)
        {
        }

        public DotGitException()
        {
        }

        public DotGitException(string message) : base(message)
        {
        }
    }
}
