using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DotGit.Models
{
    [DebuggerDisplay("Name = {Name}, Email = {Email}, Date = {Date}")]
    public class Signature
    {
        public Signature(string name, string email, DateTimeOffset date)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Date = date;
        }

        public string Name { get; }
        public string Email { get; }
        public DateTimeOffset Date { get; }
    }
}
