using System;
using System.Globalization;

namespace Ressy.Identification
{
    /// <summary>
    /// Language of a resource stored in a portable executable image.
    /// </summary>
    public partial class ResourceLanguage
    {
        /// <summary>
        /// Language ID.
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ResourceLanguage"/>.
        /// </summary>
        public ResourceLanguage(ushort id) => Id = id;

        /// <inheritdoc />
        public override string ToString() => Id.ToString(CultureInfo.InvariantCulture);
    }

    public partial class ResourceLanguage
    {
        /// <summary>
        /// Neutral language, used for locale-invariant resources.
        /// </summary>
        public static ResourceLanguage Neutral { get; } = new(0);

        /// <summary>
        /// English (United States) language.
        /// </summary>
        public static ResourceLanguage EnglishUnitedStates { get; } = new(1033);
    }

    public partial class ResourceLanguage : IEquatable<ResourceLanguage>
    {
        /// <inheritdoc />
        public bool Equals(ResourceLanguage? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return obj is ResourceLanguage other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Id.GetHashCode();
    }
}