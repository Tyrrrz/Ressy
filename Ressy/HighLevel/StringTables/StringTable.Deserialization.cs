using System.Collections.Generic;
using System.IO;

namespace Ressy.HighLevel.StringTables;

public partial class StringTable
{
    internal static StringTableBlock DeserializeBlock(int blockId, byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding);

        var strings = new string[BlockSize];

        for (var i = 0; i < BlockSize; i++)
        {
            var length = reader.ReadUInt16();
            strings[i] = new string(reader.ReadChars(length));
        }

        return new StringTableBlock(blockId, strings);
    }

    internal static StringTable Deserialize(IReadOnlyList<(int BlockId, byte[] Data)> blocks)
    {
        var stringTableBlocks = new List<StringTableBlock>(blocks.Count);

        foreach (var (blockId, data) in blocks)
            stringTableBlocks.Add(DeserializeBlock(blockId, data));

        return new StringTable(stringTableBlocks);
    }
}
