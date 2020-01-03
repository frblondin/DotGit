using DotGit.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DotGit
{
    internal static class OfsDeltaReader
    {
        internal static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Create();

        internal static (byte[] Data, int Length, ObjectType) Read(Stream stream, long offset, long length, Func<long, int, ArrayPool<byte>, (byte[] Data, ObjectType Type)> packDataAccessor, bool isNestedDelta)
        {
            var baseOffset = ReadBaseOffset(stream);
            var deltaLength = length;
            using var deflateStream = stream.DeflateForZlibData(leaveOpen: true);
            var sourceSize = ReadBaseSize(deflateStream, ref deltaLength);
            var targetSize = ReadBaseSize(deflateStream, ref deltaLength);

            byte[] delta = default, @base = default;
            ObjectType baseType;
            try
            {
                delta = ReadDelta(deflateStream, (int)deltaLength);
                (@base, baseType) = packDataAccessor(offset - baseOffset, sourceSize, _arrayPool);

                return (Undeltify(@base, delta, targetSize, isNestedDelta), targetSize, baseType);
            }
            finally
            {
                ReturnArray(delta);
                ReturnArray(@base);
            }
        }

        private static byte[] ReadDelta(Stream deflateStream, int deltaLength)
        {
            var deltaBytes = _arrayPool.Rent(deltaLength);
            deflateStream.Read(deltaBytes, 0, deltaLength);
            return deltaBytes;
        }

        private static void ReturnArray(byte[] array)
        {
            if (array != null)
            {
                _arrayPool.Return(array);
            }
        }

        private static long ReadBaseOffset(Stream stream)
        {
            var @byte = stream.ReadByte();
            var result = (long)@byte & 0b0111_1111;
            while ((@byte & 0b1000_0000) != 0)
            {
                @byte = stream.ReadByte();
                result += 1L;
                result <<= 7;
                result += @byte & 0b0111_1111;
            }
            return result;
        }

        private static int ReadBaseSize(Stream stream, ref long deltaLength)
        {
            var @byte = stream.ReadByte();
            var result = @byte & 0b0111_1111;
            int counter = 0;
            while ((@byte & 0b1000_0000) != 0)
            {
                counter++;
                @byte = stream.ReadByte();
                result += (@byte & 0b0111_1111) << (7 * counter);
            }
            deltaLength -= counter + 1;
            return result;
        }

        private static byte[] Undeltify(byte[] @base, byte[] delta, int length, bool isNestedDelta)
        {
            // Goal is to benefit from the array pool - only when we know when the array can be return
            // (ie. not in root as buffer is returned)
            var result = isNestedDelta ? _arrayPool.Rent(length) : new byte[length];
            var deltaIndex = 0;
            var targetIndex = 0;
            while (deltaIndex < delta.Length)
            {
                var size = delta[deltaIndex];
                deltaIndex++;
                if ((size & 0b1000_0000) != 0)
                {
                    // Copy
                    var (copyOffset, copySize) = GetCopySize(delta, size, ref deltaIndex);

                    Array.Copy(@base, copyOffset, result, targetIndex, copySize);
                    targetIndex += copySize;
                }
                else
                {
                    // Insert
                    Array.Copy(delta, deltaIndex, result, targetIndex, size);
                    targetIndex += size;
                    deltaIndex += size;
                }
            }
            return result;
        }

        private static (int Offset, int Size) GetCopySize(byte[] delta, int size, ref int deltaIndex)
        {
            var offset = 0;
            for (var i = 0; i < 4; i++)
            {
                if ((size & (1 << i)) != 0)
                {
                    offset += delta[deltaIndex] << (8 * i);
                    deltaIndex++;
                }
            }
            var length = 0;
            for (var i = 4; i < 7; i++)
            {
                if ((size & (1 << i)) != 0)
                {
                    length += delta[deltaIndex] << (8 * i);
                    deltaIndex++;
                }
            }
            if (length == 0)
            {
                length = 0x10000;
            }
            return (offset, length);
        }
    }
}
