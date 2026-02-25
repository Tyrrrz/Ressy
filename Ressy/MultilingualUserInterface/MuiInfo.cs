using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ressy.MultilingualUserInterface;

/// <summary>
/// Multilingual user interface information stored in a MUI resource.
/// </summary>
// https://learn.microsoft.com/windows/win32/intl/mui-resource-technology
public partial class MuiInfo(
    MuiFileType fileType,
    uint systemAttributes,
    byte[] checksum,
    byte[] serviceChecksum,
    IReadOnlyList<int> typeIDFallbackList,
    IReadOnlyList<int> typeIDMainList,
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
    /// System attributes bitmask associated with the file.
    /// </summary>
    public uint SystemAttributes { get; } = systemAttributes;

    /// <summary>
    /// MD5 checksum of the resource section.
    /// Windows compares this checksum against the associated language-neutral file
    /// to verify that the MUI satellite file matches.
    /// </summary>
    public byte[] Checksum { get; } = checksum;

    /// <summary>
    /// Secondary checksum used by Windows servicing and update.
    /// Unlike <see cref="Checksum" />, it covers different data and is used to
    /// detect whether the file has been independently patched.
    /// </summary>
    public byte[] ServiceChecksum { get; } = serviceChecksum;

    /// <summary>
    /// Ordinal resource type IDs that exist only in the language-neutral (LN) file
    /// and should fall back to it at runtime.
    /// </summary>
    public IReadOnlyList<int> TypeIDFallbackList { get; } = typeIDFallbackList;

    /// <summary>
    /// Ordinal resource type IDs that exist in the language-specific satellite file.
    /// </summary>
    public IReadOnlyList<int> TypeIDMainList { get; } = typeIDMainList;

    /// <summary>
    /// Primary language name associated with this file (e.g. "en-US").
    /// Can be <c>null</c> if not specified.
    /// </summary>
    public string? Language { get; } = language;

    /// <summary>
    /// Fallback language name (e.g. "en-US") used when the primary language
    /// resources are unavailable.
    /// Can be <c>null</c> if not specified.
    /// </summary>
    public string? FallbackLanguage { get; } = fallbackLanguage;

    /// <summary>
    /// Ultimate fallback language name (e.g. "en") used as the last resort
    /// when neither the primary nor the fallback language resources are available.
    /// Can be <c>null</c> if not specified.
    /// </summary>
    public string? UltimateFallbackLanguage { get; } = ultimateFallbackLanguage;

    /// <summary>
    /// Computes the file path to the language-specific satellite (.mui) file
    /// for the given base PE file path and desired language name (e.g. "en-US").
    /// </summary>
    /// <remarks>
    /// On Windows, MUI satellite files are stored in a subdirectory named after
    /// the language, adjacent to the language-neutral PE file.
    /// For example, a base path of <c>C:\Windows\System32\notepad.exe</c> with
    /// language <c>en-US</c> yields <c>C:\Windows\System32\en-US\notepad.exe.mui</c>.
    /// </remarks>
    public static string GetSatelliteFilePath(string filePath, string language) =>
        Path.Combine(
            Path.GetDirectoryName(filePath) ?? "",
            language,
            Path.GetFileName(filePath) + ".mui"
        );
}

public partial class MuiInfo
{
    private static Encoding Encoding { get; } = Encoding.Unicode;
}
