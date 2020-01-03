using DotGit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotGit
{
    internal static class PackIndexReader
    {
        private const int HeaderLength = 8;
        private const int FanOutTableLength = 4 * 256;
        private const int ObjectNameEntryLength = 20;
        private const int PackedObjectEntryLength = 4;

        internal static long FindOffset(string repository, string name, string hash)
        {
            var buffer = new byte[4];
            using (var stream = File.OpenRead(Path.Combine(repository, "objects", "pack", name + ".idx")))
            {
                // See https://git-scm.com/docs/pack-format
                ValidateVersion(stream);

                var (start, end, totalCount) = FindRangeFromFanOut(stream, hash, buffer);
                var hashBytes = HexStringToBytes(hash);
                var objectNameBuffer = new byte[ObjectNameEntryLength];
                var index = FindHash(stream, hashBytes, buffer, objectNameBuffer, start, end);
                if (index > -1)
                {
                    return GetOffset(stream, index, totalCount, buffer);
                }
            }
            return -1L;
        }

        private static (int Start, int End, int Total) FindRangeFromFanOut(Stream stream, string hash, byte[] buffer)
        {
            var fanoutIndex = Convert.ToInt32(hash.Substring(0, 2), 16);
            int currentPos = 0, start, end;
            if (fanoutIndex == 0)
            {
                start = 0;
                end = ReadInt(stream, buffer, 4);
                currentPos++;
            }
            else
            {
                stream.Seek((fanoutIndex - 1) * 4, SeekOrigin.Current);
                var prevFanOutCount = ReadInt(stream, buffer, 4);
                var curFanOutCount = ReadInt(stream, buffer, 4);
                currentPos = fanoutIndex + 1;
                start = prevFanOutCount - 1;
                end = curFanOutCount - 1;
            }
            stream.Seek((255 - currentPos) * 4, SeekOrigin.Current);
            var totalCount = ReadInt(stream, buffer, 4);
            return (start, end, totalCount);
        }

        private static void ValidateVersion(Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            if (buffer[0] != 255 || buffer[1] != 116 || buffer[2] != 79 || buffer[3] != 99)
            {
                throw new NotSupportedException("Only index file V2 are supported.");
            }
            stream.Read(buffer, 0, 4);
            if (buffer[0] != 0 || buffer[1] != 0 || buffer[2] != 0 || buffer[3] != 2)
            {
                throw new Exception("Invalid index file version.");
            }
        }

        private static int ReadInt(Stream stream, byte[] buffer, int length)
        {
            stream.Read(buffer, 0, length);
            Array.Reverse(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        private static long ReadInt64(Stream stream, byte[] buffer, int length)
        {
            stream.Read(buffer, 0, length);
            Array.Reverse(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        private static byte[] HexStringToBytes(string str)
        {
            var res = new byte[str.Length / 2];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }
            return res;
        }

        private static string HexBytesToString(byte[] values)
        {
            var builder = new StringBuilder();
            foreach (var b in values)
            {
                builder.Append(Convert.ToString(b, 16));
            }
            return builder.ToString();
        }

        private static int FindHash(Stream stream, byte[] hash, byte[] packFileOffset, byte[] objectName, int start, int end)
        {
            while (true)
            {
                var index = (start + end) / 2;
                var offset = HeaderLength + FanOutTableLength + index * ObjectNameEntryLength;
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(objectName, 0, ObjectNameEntryLength);
                var comparison = Compare(objectName, hash);
                if (comparison == 0)
                {
                    return index;
                }
                else if (start > end)
                {
                    return -1;
                }
                else if (comparison > 0)
                {
                    end = index - 1;
                }
                else
                {
                    start = index + 1;
                }
            }
        }

        private static int Compare(byte[] a, byte[] b)
        {
            var length = a.Length <= b.Length ? a.Length : b.Length;
            for (int i = 0; i < length; i++)
            {
                if (a[i] < b[i])
                {
                    return -1;
                }
                if (a[i] > b[i])
                {
                    return 1;
                }
            }
            return 0;
        }

        private static long GetOffset(Stream stream, int index, int totalCount, byte[] buffer)
        {
            var offset4BitStart = HeaderLength + FanOutTableLength + totalCount * ObjectNameEntryLength + totalCount * PackedObjectEntryLength;
            stream.Seek(offset4BitStart + index * 4, SeekOrigin.Begin);
            var offset4Bit = ReadInt(stream, buffer, 4);
            if ((offset4Bit & 0x1000_000) == 0)
            {
                return offset4Bit;
            }
            else
            {
                var offset8BitStart = offset4BitStart + totalCount * 4;
                stream.Seek(offset8BitStart + index * 8, SeekOrigin.Begin);
                var buffer8 = new byte[8];
                return ReadInt64(stream, buffer8, 8);
            }
        }
    }
}
