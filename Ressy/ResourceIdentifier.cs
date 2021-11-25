using System;
using System.Diagnostics.CodeAnalysis;
using Ressy.Utils;

namespace Ressy
{
    /// <summary>
    /// Identifies a single resource stored in a portable executable image.
    /// </summary>
    public partial class ResourceIdentifier
    {
        /// <summary>
        /// Resource type.
        /// </summary>
        public ResourceType Type { get; }

        /// <summary>
        /// Resource name.
        /// </summary>
        public ResourceName Name { get; }

        /// <summary>
        /// Resource language.
        /// </summary>
        public Language Language { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ResourceIdentifier"/>.
        /// </summary>
        public ResourceIdentifier(ResourceType type, ResourceName name, Language language = default)
        {
            Type = type;
            Name = name;
            Language = language;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{Type} / {Name} / {Language}";
    }

    public partial class ResourceIdentifier : IEquatable<ResourceIdentifier>
    {
        /// <inheritdoc />
        public bool Equals(ResourceIdentifier? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                Type.Equals(other.Type) &&
                Name.Equals(other.Name) &&
                Language.Equals(other.Language);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((ResourceIdentifier)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Type, Name, Language);
    }
}