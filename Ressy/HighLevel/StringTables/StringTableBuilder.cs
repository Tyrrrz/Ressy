using System;
using System.Collections.Generic;
using System.Linq;

namespace Ressy.HighLevel.StringTables;

/// <summary>
/// Builder for <see cref="StringTable" />.
/// </summary>
public class StringTableBuilder
{
    private readonly Dictionary<int, string> _strings;

    /// <summary>
    /// Initializes a new instance of <see cref="StringTableBuilder" />.
    /// </summary>
    public StringTableBuilder() => _strings = new Dictionary<int, string>();

    /// <summary>
    /// Initializes a new instance of <see cref="StringTableBuilder" /> with data from an
    /// existing <see cref="StringTable" />.
    /// </summary>
    public StringTableBuilder(StringTable? existing)
    {
        _strings = existing is not null
            ? existing.Strings.ToDictionary(kv => kv.Key, kv => kv.Value)
            : new Dictionary<int, string>();
    }

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
    /// Builds a new <see cref="StringTable" /> instance.
    /// </summary>
    public StringTable Build() => new StringTable(_strings);
}
