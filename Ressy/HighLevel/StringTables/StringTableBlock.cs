using System;
using System.Collections.Generic;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Represents a single string table resource block containing exactly 16 strings.
/// </summary>
/// <remarks>
/// <para>
/// String table data in a portable executable file is organized into blocks of 16 strings each.
/// Each block is stored as a separate resource of type <see cref="ResourceType.String" />,
/// identified by the block ID as its ordinal resource name.
/// </para>
/// <para>
/// A block with ID <c>n</c> contains strings with IDs in the range
/// <c>[(n - 1) * 16, n * 16 - 1]</c>.
/// </para>
/// </remarks>
// https://learn.microsoft.com/windows/win32/menurc/stringtable-resource
public class StringTableBlock
{
    /// <summary>
    /// Block ID (1-based).
    /// Corresponds to the ordinal name of the underlying resource of type
    /// <see cref="ResourceType.String" />.
    /// </summary>
    /// <remarks>
    /// Strings in this block have IDs in the range
    /// <c>[(<see cref="BlockId" /> - 1) * 16, <see cref="BlockId" /> * 16 - 1]</c>.
    /// </remarks>
    public int BlockId { get; }

    /// <summary>
    /// The 16 strings contained in this block, indexed by their position within the block (0-15).
    /// Empty strings indicate that no string is assigned the corresponding ID.
    /// </summary>
    /// <remarks>
    /// The global string ID for index <c>i</c> is <c>(<see cref="BlockId" /> - 1) * 16 + i</c>.
    /// </remarks>
    public IReadOnlyList<string> Strings { get; }

    /// <summary>
    /// Initializes a new <see cref="StringTableBlock" />.
    /// </summary>
    /// <param name="blockId">
    /// Block ID (1-based).
    /// Corresponds to the ordinal name of the underlying resource of type
    /// <see cref="ResourceType.String" />.
    /// </param>
    /// <param name="strings">
    /// The 16 strings in this block.
    /// Must contain exactly 16 entries.
    /// Use empty strings for absent entries.
    /// </param>
    public StringTableBlock(int blockId, IReadOnlyList<string> strings)
    {
        if (strings.Count != StringTable.BlockSize)
        {
            throw new ArgumentException(
                $"String table block must contain exactly {StringTable.BlockSize} strings.",
                nameof(strings)
            );
        }

        BlockId = blockId;
        Strings = strings;
    }

    /// <summary>
    /// Gets the string with the specified ID.
    /// Returns <c>null</c> if the string is absent or if the specified ID does not belong to
    /// this block.
    /// </summary>
    public string? TryGetString(int stringId)
    {
        if (StringTable.GetBlockId(stringId) != BlockId)
            return null;

        var str = Strings[StringTable.GetBlockIndex(stringId)];
        return string.IsNullOrEmpty(str) ? null : str;
    }

    /// <summary>
    /// Gets the string with the specified ID.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the string is absent or if the specified ID does not belong to this block.
    /// </exception>
    public string GetString(int stringId) =>
        TryGetString(stringId)
        ?? throw new InvalidOperationException(
            $"String with ID '{stringId}' does not exist in this string table block."
        );
}
