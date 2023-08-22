// ReSharper disable InconsistentNaming

using System;

namespace Ressy.HighLevel.Versions;

/// <summary>
/// Operating system for which a portable executable file was designed.
/// </summary>
[Flags]
public enum FileOperatingSystem
{
    /// <summary>
    /// Operating system for which the file was designed is unknown to the system.
    /// </summary>
    Unknown = 0x00000000,

    /// <summary>
    /// File was designed for MS-DOS.
    /// </summary>
    DOS = 0x00010000,

    /// <summary>
    /// File was designed for Windows NT.
    /// </summary>
    WindowsNT = 0x00040000,

    /// <summary>
    /// File was designed for 16-bit Windows.
    /// </summary>
    Windows16 = 0x00000001,

    /// <summary>
    /// File was designed for 32-bit Windows.
    /// </summary>
    Windows32 = 0x00000004,

    /// <summary>
    /// File was designed for 16-bit OS/2.
    /// </summary>
    OS216 = 0x00020000,

    /// <summary>
    /// File was designed for 32-bit OS/2.
    /// </summary>
    OS232 = 0x00030000,

    /// <summary>
    /// File was designed for 16-bit Presentation Manager.
    /// </summary>
    PM16 = 0x00000002,

    /// <summary>
    /// File was designed for 32-bit Presentation Manager.
    /// </summary>
    PM32 = 0x00000003
}
