using System.Collections.Generic;

namespace Ressy.Versions;

/// <summary>
/// Set of version attributes bound to a specific language and code page.
/// </summary>
public class VersionAttributeTable(
    Language language,
    CodePage codePage,
    IReadOnlyDictionary<VersionAttributeName, string> attributes
)
{
    /// <summary>
    /// Language of the contained attributes.
    /// </summary>
    public Language Language { get; } = language;

    /// <summary>
    /// Code page of the contained attributes.
    /// </summary>
    public CodePage CodePage { get; } = codePage;

    /// <summary>
    /// Contained attributes.
    /// </summary>
    public IReadOnlyDictionary<VersionAttributeName, string> Attributes { get; } = attributes;
}
