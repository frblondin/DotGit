using DotGit.Models;
using DotGit.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotGit
{
    internal static class CommitReader
    {
        private static readonly Regex _signatureRegex = new Regex(@"^\s*(.*)\s*<(.*)>\s*(\d*)\s*([+|-])(\d{4})\s*$", RegexOptions.Compiled);

        internal static Commit Read(RepositoryReader reader, string hash, Stream stream)
        {
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                var line = streamReader.ReadLine();
                var tree = StringUtils.ReadSingleValue(line, "tree ", throwIfMissing: true);
                line = streamReader.ReadLine();
                var parents = new List<string>();
                string parent;
                while ((parent = StringUtils.ReadSingleValue(line, "parent ", throwIfMissing: false)) != null)
                {
                    parents.Add(parent);
                    line = streamReader.ReadLine();
                }
                var author = StringUtils.ReadSingleValue(line, "author ", throwIfMissing: true);
                line = streamReader.ReadLine();
                var committer = StringUtils.ReadSingleValue(line, "committer ", throwIfMissing: true);
                streamReader.ReadLine();
                var message = streamReader.ReadToEnd().Trim();
                return new Commit(
                    hash,
                    new Lazy<TreeCollection>(() => reader.Read<TreeCollection>(tree)),
                    new Lazy<IImmutableList<Commit>>(() => parents.Select(p => reader.Read<Commit>(p)).ToImmutableList()),
                    ParseSignature(author),
                    ParseSignature(committer), message);
            }
        }

        private static Signature ParseSignature(string message)
        {
            var authorMatch = _signatureRegex.Match(message);
            if (!authorMatch.Success)
            {
                throw new Exception($"Signature string in could not be parsed.");
            }
            return new Signature(
                authorMatch.Groups[1].Value.Trim(),
                authorMatch.Groups[2].Value,
                ReadDateTime(authorMatch.Groups[3].Value, authorMatch.Groups[4].Value, authorMatch.Groups[5].Value));
        }

        private static DateTimeOffset ReadDateTime(string unixTime, string offsetSign, string rawOffset)
        {
            var result = DateTimeOffset.FromUnixTimeSeconds(long.Parse(unixTime, CultureInfo.InvariantCulture));
            var offset = TimeSpan.ParseExact(rawOffset, "hhmm", CultureInfo.InvariantCulture);
            if (offsetSign == "-")
            {
                offset = offset.Negate();
            }
            return result.ToOffset(offset);
        }
    }
}
