using System;
using System.IO;
using System.Text;

namespace Ressy.Utils.Extensions
{
    internal static class BinaryReaderExtensions
    {
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

        public static Version ReadVersion(this BinaryReader reader)
        {
            var (major, minor) = BitPack.Split(reader.ReadUInt32());
            var (build, revision) = BitPack.Split(reader.ReadUInt32());

            return new Version(major, minor, build, revision);
        }
    }
}