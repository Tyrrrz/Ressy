using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Contains strings loaded from string table resources, keyed by their IDs.
/// </summary>
// https://learn.microsoft.com/windows/win32/menurc/stringtable-resource
public partial class StringTable : IReadOnlyDictionary<int, string>
{
    private readonly IReadOnlyDictionary<int, string> _strings;

    /// <summary>
    /// Initializes a new instance of <see cref="StringTable" />.
    /// </summary>
    public StringTable(IReadOnlyDictionary<int, string> strings) => _strings = strings;

    /// <summary>
    /// Gets the string with the specified ID.
    /// Returns <c>null</c> if the string does not exist.
    /// </summary>
    public string? TryGetString(int stringId) =>
        _strings.TryGetValue(stringId, out var value) ? value : null;

    /// <summary>
    /// Gets the string with the specified ID.
    /// </summary>
    public string GetString(int stringId) =>
        TryGetString(stringId)
        ?? throw new InvalidOperationException(
            $"String with ID '{stringId}' does not exist in the string table."
        );

    /// <inheritdoc />
    public string this[int key] => _strings[key];

    /// <inheritdoc />
    public IEnumerable<int> Keys => _strings.Keys;

    /// <inheritdoc />
    public IEnumerable<string> Values => _strings.Values;

    /// <inheritdoc />
    public int Count => _strings.Count;

    /// <inheritdoc />
    public bool ContainsKey(int key) => _strings.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGetValue(int key, out string value)
    {
        if (_strings.TryGetValue(key, out var result))
        {
            value = result!;
            return true;
        }
        value = default!;
        return false;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<int, string>> GetEnumerator() => _strings.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_strings).GetEnumerator();
}

public partial class StringTable
{
    internal const int BlockSize = 16;

    internal static int GetBlockId(int stringId) => (stringId >> 4) + 1;

    internal static int GetBlockIndex(int stringId) => stringId & 0x0F;

    private static Encoding Encoding { get; } = Encoding.Unicode;
}
