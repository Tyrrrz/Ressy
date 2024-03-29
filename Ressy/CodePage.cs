using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Ressy;

/// <summary>
/// Code page identifier that specifies the text encoding of a resource.
/// </summary>
public readonly partial struct CodePage(int id)
{
    /// <summary>
    /// Codepage ID.
    /// </summary>
    public int Id { get; } = id;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"ID: {Id}";
}

public partial struct CodePage
{
    /// <summary>
    /// Unicode (UTF-16) code page.
    /// </summary>
    public static CodePage Unicode => new(1200);

    /// <summary>
    /// Creates a code page identifier based on the specified encoding.
    /// </summary>
    public static CodePage FromEncoding(Encoding encoding) => new(encoding.CodePage);
}

public partial struct CodePage : IEquatable<CodePage>
{
    /// <inheritdoc />
    public bool Equals(CodePage other) => Id == other.Id;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CodePage other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Id;
}
