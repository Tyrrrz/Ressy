namespace Ressy.HighLevel.Versions;

/// <summary>
/// Type of a portable executable file.
/// </summary>
public enum FileType
{
    /// <summary>
    /// File type is unknown to the system.
    /// </summary>
    Unknown = 0x00000000,

    /// <summary>
    /// File contains an application.
    /// </summary>
    Application = 0x00000001,

    /// <summary>
    /// File contains a DLL.
    /// </summary>
    DynamicallyLinkedLibrary = 0x00000002,

    /// <summary>
    /// File contains a device driver.
    /// </summary>
    Driver = 0x00000003,

    /// <summary>
    /// File contains a font.
    /// </summary>
    Font = 0x00000004,

    /// <summary>
    /// File contains a static-link library.
    /// </summary>
    StaticallyLinkedLibrary = 0x00000007,

    /// <summary>
    /// File contains a virtual device.
    /// </summary>
    VirtualDevice = 0x00000005,
}
