using System;
using System.Collections.Generic;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Builder for <see cref="StringTable" />.
/// </summary>
public class StringTableBuilder
{
    private readonly Dictionary<int, string> _strings = new();

    /// <summary>
    /// Sets the string with the specified ID.
    /// </summary>
    public StringTableBuilder SetString(int stringId, string value)
    {
        if (stringId < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stringId),
                "String ID must be non-negative."
            );
        }

        _strings[stringId] = value;
        return this;
    }

    /// <summary>
    /// Copies all data from an existing <see cref="StringTable" /> instance.
    /// </summary>
    public StringTableBuilder SetAll(StringTable existing)
    {
        foreach (var (key, value) in existing.Strings)
            _strings[key] = value;

        return this;
    }

    /// <summary>
    /// Builds a new <see cref="StringTable" /> instance.
    /// </summary>
    public StringTable Build() => new StringTable(_strings);
}
