using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ressy
{
    /// <summary>
    /// Language of a resource stored in a portable executable image.
    /// </summary>
    public readonly partial struct ResourceLanguage
    {
        /// <summary>
        /// Language ID (LCID).
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
        public static ResourceLanguage FromCultureInfo(CultureInfo cultureInfo) => new(cultureInfo.LCID & 0xffff);
    }
}