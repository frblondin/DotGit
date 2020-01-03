using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace System.IO
{
    internal static class StreamExtensions
    {
        internal static string ReadUpTo(this Stream stream, char delimiter)
        {
            var result = new StringBuilder();
            var oneByteBuffer = new byte[1];
            char ch;
            while (stream.Read(oneByteBuffer, 0, 1) > 0 &&
                (ch = Encoding.UTF8.GetChars(oneByteBuffer)[0]) != delimiter)
            {
                result.Append(ch);
            }
            return result.ToString();
        }

        internal static DeflateStream DeflateForZlibData(this Stream stream, bool leaveOpen = false)
        {
            stream.Seek(2, SeekOrigin.Current);
            return new DeflateStream(stream, CompressionMode.Decompress, leaveOpen);
        }
    }
}
