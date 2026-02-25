using System.Text;

namespace Ressy.MultilingualUserInterface;

/// <summary>
/// Multilingual user interface information associated with a portable executable file.
/// </summary>
// https://learn.microsoft.com/windows/win32/intl/mui-resource-technology
public partial class MultilingualUserInterfaceInfo(
    MuiFileType fileType,
    string? language,
    string? fallbackLanguage,
    string? ultimateFallbackLanguage
)
{
    /// <summary>
    /// File type indicating whether this is a language-neutral or language-specific resource.
    /// </summary>
    public MuiFileType FileType { get; } = fileType;

    /// <summary>
    /// Language name associated with this file (e.g. "en-US").
    /// Can be <c>null</c> if not specified.
    /// </summary>
    public string? Language { get; } = language;

    /// <summary>
    /// Fallback language name (e.g. "en-US").
    /// Can be <c>null</c> if not specified.
    /// </summary>
    public string? FallbackLanguage { get; } = fallbackLanguage;

    /// <summary>
    /// Ultimate fallback language name (e.g. "en").
    /// Can be <c>null</c> if not specified.
    /// </summary>
    public string? UltimateFallbackLanguage { get; } = ultimateFallbackLanguage;
}

public partial class MultilingualUserInterfaceInfo
{
    private static Encoding Encoding { get; } = Encoding.Unicode;
}
