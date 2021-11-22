namespace Ressy.Abstractions.Versions
{
    /// <summary>
    /// Specifies language and codepage that the application or library supports.
    /// </summary>
    public class TranslationInfo
    {
        /// <summary>
        /// Language ID (LCID).
        /// </summary>
        public int LanguageId { get; }

        /// <summary>
        /// Codepage ID.
        /// </summary>
        public int Codepage { get; }

        /// <summary>
        /// Initializes an instance of <see cref="TranslationInfo"/>.
        /// </summary>
        public TranslationInfo(int languageId, int codepage)
        {
            LanguageId = languageId;
            Codepage = codepage;
        }
    }
}