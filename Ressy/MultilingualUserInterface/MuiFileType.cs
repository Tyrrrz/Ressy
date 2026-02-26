namespace Ressy.MultilingualUserInterface;

/// <summary>
/// File type of a MUI resource, indicating whether the file is language-neutral
/// or language-specific.
/// </summary>
// https://learn.microsoft.com/windows/win32/intl/mui-resource-technology
public enum MuiFileType
{
    /// <summary>
    /// Language-neutral file that contains resources common to all languages.
    /// </summary>
    LanguageNeutral = 0x11,

    /// <summary>
    /// Language-specific satellite file that contains localizable resources.
    /// </summary>
    LanguageSpecific = 0x12,
}
