using System.Collections.Generic;

namespace Ressy.HighLevel.StringTables;

public partial class StringTable
{
    internal static StringTable Deserialize(IReadOnlyDictionary<int, byte[]> blocks)
    {
        var stringTableBlocks = new List<StringTableBlock>(blocks.Count);

        foreach (var (blockId, data) in blocks)
            stringTableBlocks.Add(StringTableBlock.Deserialize(blockId, data));

        return FromBlocks(stringTableBlocks);
    }
}
