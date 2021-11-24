using System;
using System.IO;
using System.Text;

namespace Ressy.Utils.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static bool IsEndOfStream(this BinaryReader reader) =>
            reader.BaseStream.Position >= reader.BaseStream.Length;

        public static void SkipPadding(this BinaryReader reader, int bytes = 4)
        {
            var remainder = reader.BaseStream.Position % bytes;

            // Already on the boundary
            if (remainder == 0)
                return;

            var padding = Math.Min(bytes - remainder, reader.BaseStream.Length - reader.BaseStream.Position);
            reader.BaseStream.Seek(padding, SeekOrigin.Current);
        }

        public static void SkipZeroes(this BinaryReader reader, long? maxSkipLength = null)
        {
            var endPosition = maxSkipLength is not null
                ? reader.BaseStream.Position + maxSkipLength
                : reader.BaseStream.Length;

            while (reader.BaseStream.Position < endPosition)
            {
                if (reader.ReadByte() != 0)
                {
                    // Go back to non-zero byte
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);
                    return;
                }
            }
        }

        public static string ReadStringNullTerminated(this BinaryReader reader)
        {
            var buffer = new StringBuilder();

            while (true)
            {
                var c = reader.ReadChar();
                if (c == '\0')
                    break;

                buffer.Append(c);
            }

            return buffer.ToString();
        }
    }
}