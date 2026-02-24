using System;
using System.Collections.Generic;
using System.Text;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Contains strings loaded from string table resources, keyed by their IDs.
/// </summary>
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
}

public partial class StringTable
{
    internal const int BlockSize = 16;

    internal static int GetBlockId(int stringId) => (stringId >> 4) + 1;

    internal static int GetBlockIndex(int stringId) => stringId & 0x0F;

    private static Encoding Encoding { get; } = Encoding.Unicode;
}
