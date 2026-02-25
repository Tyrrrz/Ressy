using System;

namespace Ressy.Versions;

/// <summary>
/// Flags associated with a portable executable file.
/// </summary>
[Flags]
public enum FileFlags
{
    /// <summary>
    /// File does not have any special flags.
    /// </summary>
    None = 0x00000000,

    /// <summary>
    /// File contains debugging information or is compiled with debugging features enabled.
    /// </summary>
    Debug = 0x00000001,

    /// <summary>
    /// File's version structure was created dynamically;
    /// therefore, some of the members in this structure may be empty or incorrect.
    /// </summary>
    InfoInferred = 0x00000010,

    /// <summary>
    /// File has been modified and is not identical to the original shipping file of the same version number.
    /// </summary>
    Patched = 0x00000004,

    /// <summary>
    /// File is a development version, not a commercially released product.
    /// </summary>
    PreRelease = 0x00000002,

    /// <summary>
    /// File was not built using standard release procedures.
    /// </summary>
    PrivateBuild = 0x00000008,

    /// <summary>
    /// File was built by the original company using standard release procedures
    /// but is a variation of the normal file of the same version number.
    /// </summary>
    SpecialBuild = 0x00000020,
}
