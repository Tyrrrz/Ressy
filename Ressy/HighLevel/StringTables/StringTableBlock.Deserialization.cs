using System.IO;

namespace Ressy.HighLevel.StringTables;

public partial class StringTableBlock
{
    internal static StringTableBlock Deserialize(int blockId, byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, StringTable.Encoding);

        var strings = new string[StringTable.BlockSize];

        for (var i = 0; i < StringTable.BlockSize; i++)
        {
            var length = reader.ReadUInt16();
            strings[i] = new string(reader.ReadChars(length));
        }

        return new StringTableBlock(blockId, strings);
    }
}
