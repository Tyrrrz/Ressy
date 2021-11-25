using System.IO;
using System.Text;

namespace Ressy.Utils.Extensions
{
    internal static class BinaryReaderExtensions
    {
        public static bool IsEndOfStream(this BinaryReader reader) =>
            reader.BaseStream.Position >= reader.BaseStream.Length;

        public static void SkipPadding(this BinaryReader reader, int boundaryBits = 32)
        {
            while (!reader.IsEndOfStream() && reader.BaseStream.Position * 8 % boundaryBits != 0)
            {
                // Read a character so that it takes up either 1 or 2 bytes,
                // depending on the encoding of the stream.
                _ = reader.ReadChar();
            }
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

        public static string ReadNullTerminatedString(this BinaryReader reader)
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