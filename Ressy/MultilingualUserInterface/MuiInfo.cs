using System;
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
    byte[] checksum,
    byte[] serviceChecksum,
    IReadOnlyList<ResourceType> mainResourceTypes,
    IReadOnlyList<ResourceType> fallbackResourceTypes,
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
    /// Ordinal resource types that remain in the main (language-neutral) file.
    /// </summary>
    public IReadOnlyList<ResourceType> MainResourceTypes { get; } = mainResourceTypes;

    /// <summary>
    /// Ordinal resource types that are split into the language-specific satellite (.mui) file.
    /// </summary>
    public IReadOnlyList<ResourceType> FallbackResourceTypes { get; } = fallbackResourceTypes;

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
    /// for the given base PE file path, using the <see cref="Language" /> of this resource.
    /// </summary>
    /// <remarks>
    /// MUI satellite files are stored in a subdirectory named after
    /// the language, adjacent to the language-neutral PE file.
    /// For example, a base path of <c>C:\Windows\System32\notepad.exe</c> with
    /// language <c>en-US</c> yields <c>C:\Windows\System32\en-US\notepad.exe.mui</c>.
    /// </remarks>
    public string GetSatelliteFilePath(string filePath) =>
        Language is not null
            ? Path.Combine(
                Path.GetDirectoryName(filePath) ?? "",
                Language,
                Path.GetFileName(filePath) + ".mui"
            )
            : throw new InvalidOperationException(
                "Cannot compute satellite file path: Language is not set."
            );
}

public partial class MuiInfo
{
    private static Encoding Encoding { get; } = Encoding.Unicode;
}
