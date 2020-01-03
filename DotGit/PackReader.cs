using DotGit.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGit
{
    internal abstract class PackReader
    {
        private const int HeaderInfoChunkSize = 4;

        public static T Read<T>(RepositoryReader reader, string repository, string hash)
            where T : Entry
        {
            foreach (var index in Directory.EnumerateFiles(Path.Combine(repository, "objects", "pack"), "*.idx"))
            {
                var pack = Path.GetFileNameWithoutExtension(index);
                var offset = PackIndexReader.FindOffset(repository, pack, hash);
                if (offset != -1L)
                {
                    return (T)Read(reader, repository, pack, hash, offset);
                }
            }
            return null;
        }

        private static Entry Read(RepositoryReader reader, string repository, string name, string hash, long offset)
        {
            var path = Path.Combine(repository, "objects", "pack", name + ".pack");
            using (var stream = File.OpenRead(path))
            {
                // See https://git-scm.com/docs/pack-format
                var (version, objectCount) = ReadHeader(stream);
                return ReadEntry(reader, hash, stream, offset, () => File.OpenRead(path));
            }
        }

        private static (int Version, int ObjectCount) ReadHeader(Stream stream)
        {
            var buffer = new byte[4];
            var signature = ReadString(stream, buffer, HeaderInfoChunkSize);
            Debug.Assert(signature == "PACK", "Pack header should contain 'pack'.");
            var version = ReadInt(stream, buffer, HeaderInfoChunkSize);
            var objectCount = ReadInt(stream, buffer, HeaderInfoChunkSize);
            return (version, objectCount);
        }

        private static Entry ReadEntry(RepositoryReader reader, string hash, Stream stream, long offset, Func<Stream> streamProvider)
        {
            var (type, length) = ReadEntryInformation(stream, offset);
            switch (type)
            {
                case ObjectType.Commit:
                case ObjectType.Tree:
                    using (var deflateStream = stream.DeflateForZlibData())
                    {
                        return ReadEntryStream(reader, hash, stream, deflateStream, offset, type, length, streamProvider);
                    }
                case ObjectType.Blob:
                case ObjectType.OfsDelta:
                    return ReadEntryStream(reader, hash, stream, stream, offset, type, length, streamProvider);
                default:
                    throw new NotImplementedException();
            }
        }

        private static Entry ReadEntryStream(RepositoryReader reader, string hash, Stream stream, Stream entryStream, long offset, ObjectType type, long length, Func<Stream> streamProvider)
        {
            switch (type)
            {
                case ObjectType.Commit:
                    return CommitReader.Read(reader, hash, entryStream);
                case ObjectType.Tree:
                    return TreeReader.Read(reader, hash, entryStream, length);
                case ObjectType.Blob:
                    return BlobReader.Read(reader, hash, entryStream, streamProvider);
                case ObjectType.OfsDelta:
                    var (undeltifiedData, undeltifiedLength, baseType) = OfsDeltaReader.Read(
                        stream, offset, length,
                        (off, len, pool) => ReadEntryData(reader, hash, stream, off, isNestedDelta: true),
                        isNestedDelta: false);
                    using (var undeltifiedStream = new MemoryStream(undeltifiedData))
                    {
                        return ReadEntryStream(reader, hash, stream, undeltifiedStream, 0L, baseType, undeltifiedLength, streamProvider);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private static (byte[] Data, ObjectType Type) ReadEntryData(RepositoryReader reader, string hash, Stream stream, long offset, bool isNestedDelta)
        {
            var (type, length) = ReadEntryInformation(stream, offset);
            byte[] result;
            switch (type)
            {
                case ObjectType.Commit:
                case ObjectType.Tree:
                    using (var deflateStream = stream.DeflateForZlibData())
                    {
                        result = isNestedDelta ? OfsDeltaReader._arrayPool.Rent((int)length) : new byte[length];
                        deflateStream.Read(result, 0, (int)length);
                    }
                    break;
                case ObjectType.Blob:
                    result = isNestedDelta ? OfsDeltaReader._arrayPool.Rent((int)length) : new byte[length];
                    stream.Read(result, 0, (int)length);
                    break;
                case ObjectType.OfsDelta:
                    (result, _, type) = OfsDeltaReader.Read(stream, offset, length,
                        (off, len, pool) => ReadEntryData(reader, hash, stream, off, isNestedDelta), isNestedDelta);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return (result, type);
        }

        private static (ObjectType Type, long Length) ReadEntryInformation(Stream stream, long offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            var @byte = stream.ReadByte();
            var type = (ObjectType)((@byte & 0b0111_0000) >> 4);
            var length = ComputeLength(stream, @byte);
            return (type, length);
        }

        private static long ComputeLength(Stream stream, int @byte)
        {
            long length = @byte & 0b0000_1111;
            int counter = 0;
            while ((@byte & 0b1000_0000) != 0)
            {
                counter++;
                @byte = stream.ReadByte();
                length += (@byte & 0b0111_1111) << (4 + (7 * (counter - 1)));
            }

            return length;
        }

        private static string ReadString(Stream stream, byte[] buffer, int length)
        {
            stream.Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

        private static int ReadInt(Stream stream, byte[] buffer, int length)
        {
            stream.Read(buffer, 0, length);
            Array.Reverse(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}
