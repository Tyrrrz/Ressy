using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Ressy.Utils;

namespace Ressy;

/// <summary>
/// Language identifier that specifies the locale of a resource or the text contained within it.
/// </summary>
public readonly partial struct Language
{
    /// <summary>
    /// Language ID.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Initializes an instance of <see cref="Language" />.
    /// </summary>
    public Language(int id) => Id = id;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"ID: {Id}";
}

public partial struct Language
{
    /// <summary>
    /// Neutral language.
    /// </summary>
    public static Language Neutral { get; } = new(0);

    /// <summary>
    /// Creates a language identifier from a culture descriptor.
    /// </summary>
    // https://docs.microsoft.com/en-us/windows/win32/intl/locale-identifiers
    public static Language FromCultureInfo(CultureInfo cultureInfo)
    {
        var (_, languageId) = BitPack.Split(cultureInfo.LCID);
        return new Language(languageId);
    }
}

public partial struct Language : IEquatable<Language>
{
    /// <inheritdoc />
    public bool Equals(Language other) => Id == other.Id;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Language other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Id;
}