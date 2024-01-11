using System;
using System.Diagnostics.CodeAnalysis;

namespace Ressy;

/// <summary>
/// Identifies a single resource stored in a portable executable image.
/// </summary>
public partial class ResourceIdentifier(
    ResourceType type,
    ResourceName name,
    Language language = default
)
{
    /// <summary>
    /// Resource type.
    /// </summary>
    public ResourceType Type { get; } = type;

    /// <summary>
    /// Resource name.
    /// </summary>
    public ResourceName Name { get; } = name;

    /// <summary>
    /// Resource language.
    /// </summary>
    public Language Language { get; } = language;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Type} / {Name} / {Language}";
}

public partial class ResourceIdentifier : IEquatable<ResourceIdentifier>
{
    /// <inheritdoc />
    public bool Equals(ResourceIdentifier? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Type.Equals(other.Type)
            && Name.Equals(other.Name)
            && Language.Equals(other.Language);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((ResourceIdentifier)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Type, Name, Language);
}
