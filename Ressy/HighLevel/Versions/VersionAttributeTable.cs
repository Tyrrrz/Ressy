using System.Collections.Generic;

namespace Ressy.HighLevel.Versions
{
    /// <summary>
    /// Set of version attributes bound to a specific language and code page.
    /// </summary>
    public class VersionAttributeTable
    {
        /// <summary>
        /// Language of the contained attributes.
        /// </summary>
        public Language Language { get; }

        /// <summary>
        /// Code page of the contained attributes.
        /// </summary>
        public CodePage CodePage { get; }

        /// <summary>
        /// Contained attributes.
        /// </summary>
        public IReadOnlyDictionary<VersionAttributeName, string> Attributes { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VersionAttributeTable"/>.
        /// </summary>
        public VersionAttributeTable(
            Language language,
            CodePage codePage,
            IReadOnlyDictionary<VersionAttributeName, string> attributes)
        {
            Language = language;
            CodePage = codePage;
            Attributes = attributes;
        }
    }
}