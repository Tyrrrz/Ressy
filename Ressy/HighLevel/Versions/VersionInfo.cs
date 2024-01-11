using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ressy.HighLevel.Versions;

/// <summary>
/// Version information associated with a portable executable file.
/// </summary>
// https://docs.microsoft.com/en-us/windows/win32/menurc/vs-versioninfo
public partial class VersionInfo(
    Version fileVersion,
    Version productVersion,
    FileFlags fileFlags,
    FileOperatingSystem fileOperatingSystem,
    FileType fileType,
    FileSubType fileSubType,
    IReadOnlyList<VersionAttributeTable> attributeTables
)
{
    /// <summary>
    /// File version.
    /// </summary>
    public Version FileVersion { get; } = fileVersion;

    /// <summary>
    /// Product version.
    /// </summary>
    public Version ProductVersion { get; } = productVersion;

    /// <summary>
    /// File flags.
    /// </summary>
    public FileFlags FileFlags { get; } = fileFlags;

    /// <summary>
    /// File's target operating system.
    /// </summary>
    public FileOperatingSystem FileOperatingSystem { get; } = fileOperatingSystem;

    /// <summary>
    /// File type.
    /// </summary>
    public FileType FileType { get; } = fileType;

    /// <summary>
    /// File sub-type.
    /// </summary>
    public FileSubType FileSubType { get; } = fileSubType;

    /// <summary>
    /// Version attribute tables.
    /// </summary>
    public IReadOnlyList<VersionAttributeTable> AttributeTables { get; } = attributeTables;

    /// <summary>
    /// Gets the value of the specified attribute.
    /// Returns <c>null</c> if the specified attribute doesn't exist in any of the attribute tables.
    /// </summary>
    /// <remarks>
    /// If version info includes multiple attribute tables, this method retrieves the value from the
    /// first table that contains the specified attribute, giving preference to tables in the
    /// neutral language.
    /// </remarks>
    public string? TryGetAttribute(VersionAttributeName name) =>
        AttributeTables
            .OrderBy(t => t.Language.Id == Language.Neutral.Id)
            .Select(t => t.Attributes.GetValueOrDefault(name))
            .FirstOrDefault(s => s is not null);

    /// <summary>
    /// Gets the value of the specified attribute.
    /// </summary>
    /// <remarks>
    /// If version info includes multiple attribute tables, this method retrieves the value from the
    /// first table that contains the specified attribute, giving preference to tables in the
    /// neutral language.
    /// </remarks>
    public string GetAttribute(VersionAttributeName name) =>
        TryGetAttribute(name)
        ?? throw new InvalidOperationException(
            $"Attribute '{name}' does not exist in any of the attribute tables."
        );
}

public partial class VersionInfo
{
    private static Encoding Encoding { get; } = Encoding.Unicode;
}
