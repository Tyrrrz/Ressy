namespace Ressy.HighLevel.Versions
{
    /// <summary>
    /// Sub-type of a portable executable file.
    /// </summary>
    public enum FileSubType
    {
        /// <summary>
        /// File sub-type is unknown to the system.
        /// </summary>
        Unknown = 0x00000000,

        /// <summary>
        /// File contains a communications driver.
        /// </summary>
        DriverCommunications = 0x0000000A,

        /// <summary>
        /// File contains a display driver.
        /// </summary>
        DriverDisplay = 0x00000004,

        /// <summary>
        /// File contains an installable driver.
        /// </summary>
        DriverInstallable = 0x00000008,

        /// <summary>
        /// File contains a keyboard driver.
        /// </summary>
        DriverKeyboard = 0x00000002,

        /// <summary>
        /// File contains a language driver.
        /// </summary>
        DriverLanguage = 0x00000003,

        /// <summary>
        /// File contains a mouse driver.
        /// </summary>
        DriverMouse = 0x00000005,

        /// <summary>
        /// File contains a network driver.
        /// </summary>
        DriverNetwork = 0x00000006,

        /// <summary>
        /// File contains a printer driver.
        /// </summary>
        DriverPrinter = 0x00000001,

        /// <summary>
        /// File contains a sound driver.
        /// </summary>
        DriverSound = 0x00000009,

        /// <summary>
        /// File contains a system driver.
        /// </summary>
        DriverSystem = 0x00000007,

        /// <summary>
        /// File contains a versioned printer driver.
        /// </summary>
        DriverVersionedPrinter = 0x0000000C,

        /// <summary>
        /// File contains a raster font.
        /// </summary>
        FontRaster = 0x00000001,

        /// <summary>
        /// File contains a TrueType font.
        /// </summary>
        FontTrueType = 0x00000003,

        /// <summary>
        /// File contains a vector font.
        /// </summary>
        FontVector = 0x00000002
    }
}