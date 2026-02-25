using System.Collections.Generic;
using System.Linq;

namespace Ressy.HighLevel.StringTables;

public partial class StringTable
{
    /// <summary>
    /// Converts the string table into a list of resource blocks.
    /// </summary>
    /// <remarks>
    /// Strings are distributed across blocks according to their IDs.
    /// Blocks are filled with empty strings for absent entries.
    /// </remarks>
    public IReadOnlyList<StringTableBlock> ToBlocks()
    {
        if (Strings.Count == 0)
            return [];

        var blocks = new List<StringTableBlock>();

        // Find the highest block ID needed
        var maxBlockId = Strings.Keys.Max(StringTableBlock.GetBlockId);

        // Generate all blocks from 1 to maxBlockId
        for (var blockId = 1; blockId <= maxBlockId; blockId++)
        {
            // Create a block filled with empty strings.
            // Null strings are not allowed in string table resources.
            var blockStrings = Enumerable
                .Repeat(string.Empty, StringTableBlock.BlockSize)
                .ToArray();

            // Fill in strings for this block
            foreach (var (stringId, str) in Strings)
            {
                if (StringTableBlock.GetBlockId(stringId) != blockId)
                    continue;

                blockStrings[StringTableBlock.GetBlockIndex(stringId)] = str;
            }

            blocks.Add(new StringTableBlock(blockId, blockStrings));
        }

        return blocks;
    }
}
