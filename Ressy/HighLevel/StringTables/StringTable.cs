using System;
using System.Collections.Generic;
using System.Linq;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Contains strings loaded from string table resource blocks, keyed by their IDs.
/// </summary>
/// <remarks>
/// String table data is stored in a portable executable file as a series of resource blocks,
/// each containing 16 strings (see <see cref="StringTableBlock" />).
/// This class provides a unified view over all blocks.
/// </remarks>
// https://learn.microsoft.com/windows/win32/menurc/stringtable-resource
public partial class StringTable(IReadOnlyDictionary<int, string> strings)
{
    /// <summary>
    /// Contains the string entries, keyed by their IDs.
    /// </summary>
    public IReadOnlyDictionary<int, string> Strings { get; } = strings;

    /// <summary>
    /// Gets the string with the specified ID.
    /// Returns <c>null</c> if the string doesn't exist.
    /// </summary>
    public string? TryGetString(int stringId) =>
        Strings.TryGetValue(stringId, out var value) ? value : null;

    /// <summary>
    /// Gets the string with the specified ID.
    /// </summary>
    public string GetString(int stringId) =>
        TryGetString(stringId)
        ?? throw new InvalidOperationException(
            $"String with ID '{stringId}' does not exist in the string table."
        );

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

        var maxBlockId = Strings.Keys.Max(StringTableBlock.GetBlockId);

        // Pre-allocate one string array per block, filled with empty strings.
        // Null strings are not allowed in string table resources.
        var blockStrings = new string[maxBlockId][];
        for (var i = 0; i < maxBlockId; i++)
            blockStrings[i] = Enumerable.Repeat(string.Empty, StringTableBlock.BlockSize).ToArray();

        // Distribute all strings into the correct blocks in a single pass
        foreach (var (stringId, value) in Strings)
        {
            blockStrings[StringTableBlock.GetBlockId(stringId) - 1][
                StringTableBlock.GetBlockIndex(stringId)
            ] = value;
        }

        return blockStrings
            .Select((strings, i) => new StringTableBlock(i + 1, strings))
            // Skip fully empty blocks
            .Where(block => block.Strings.Any(str => !string.IsNullOrEmpty(str)))
            .ToArray();
    }
}

public partial class StringTable
{
    /// <summary>
    /// Creates a new <see cref="StringTable" /> from a collection of string table resource blocks.
    /// </summary>
    public static StringTable FromBlocks(IReadOnlyList<StringTableBlock> blocks)
    {
        var strings = new Dictionary<int, string>();

        foreach (var block in blocks)
        {
            foreach (var (stringId, value) in block.Strings.Index())
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                strings[(block.BlockId - 1) * StringTableBlock.BlockSize + stringId] = value;
            }
        }

        return new StringTable(strings);
    }
}
