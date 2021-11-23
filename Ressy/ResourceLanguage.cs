using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Ressy.Utils;

namespace Ressy
{
    /// <summary>
    /// Language of a resource stored in a portable executable image.
    /// </summary>
    public readonly partial struct ResourceLanguage
    {
        /// <summary>
        /// Language ID.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ResourceLanguage"/>.
        /// </summary>
        public ResourceLanguage(int id) => Id = id;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => Id.ToString(CultureInfo.InvariantCulture);
    }

    public partial struct ResourceLanguage
    {
        /// <summary>
        /// Neutral language, used for locale-invariant resources.
        /// </summary>
        public static ResourceLanguage Neutral { get; } = new(0);

        /// <summary>
        /// Creates a language identifier from a culture descriptor.
        /// </summary>
        // https://docs.microsoft.com/en-us/windows/win32/intl/locale-identifiers?redirectedfrom=MSDN
        public static ResourceLanguage FromCultureInfo(CultureInfo cultureInfo)
        {
            var (_, languageId) = BitPack.Split(cultureInfo.LCID);
            return new ResourceLanguage(languageId);
        }
    }

    public partial struct ResourceLanguage : IEquatable<ResourceLanguage>
    {
        /// <inheritdoc />
        public bool Equals(ResourceLanguage other) => Id == other.Id;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ResourceLanguage other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Id;
    }
}