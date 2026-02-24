using System.IO;

namespace Ressy.HighLevel.StringTables;

internal partial class StringTable
{
    internal static string[] Deserialize(byte[] data)
    {
        var strings = new string[BlockSize];

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding);

        for (var i = 0; i < BlockSize; i++)
        {
            var length = reader.ReadUInt16();
            strings[i] = new string(reader.ReadChars(length));
        }

        return strings;
    }
}
