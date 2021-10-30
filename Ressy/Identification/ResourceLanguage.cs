using System.Diagnostics.CodeAnalysis;
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
        [ExcludeFromCodeCoverage]
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
}