namespace Ressy.Abstractions.Versions
{
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
        App = 0x00000001,

        /// <summary>
        /// File contains a DLL.
        /// </summary>
        Dll = 0x00000002,

        /// <summary>
        /// File contains a device driver.
        /// </summary>
        Drv = 0x00000003,

        /// <summary>
        /// File contains a font.
        /// </summary>
        Font = 0x00000004,

        /// <summary>
        /// File contains a static-link library.
        /// </summary>
        StaticLib = 0x00000007,

        /// <summary>
        /// File contains a virtual device.
        /// </summary>
        Vxd = 0x00000005
    }
}