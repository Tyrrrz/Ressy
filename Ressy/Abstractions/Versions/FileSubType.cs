namespace Ressy.Abstractions.Versions
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
        DrvComm = 0x0000000A,

        /// <summary>
        /// File contains a display driver.
        /// </summary>
        DrvDisplay = 0x00000004,

        /// <summary>
        /// File contains an installable driver.
        /// </summary>
        DrvInstallable = 0x00000008,

        /// <summary>
        /// File contains a keyboard driver.
        /// </summary>
        DrvKeyboard = 0x00000002,

        /// <summary>
        /// File contains a language driver.
        /// </summary>
        DrvLanguage = 0x00000003,

        /// <summary>
        /// File contains a mouse driver.
        /// </summary>
        DrvMouse = 0x00000005,

        /// <summary>
        /// File contains a network driver.
        /// </summary>
        DrvNetwork = 0x00000006,

        /// <summary>
        /// File contains a printer driver.
        /// </summary>
        DrvPrinter = 0x00000001,

        /// <summary>
        /// File contains a sound driver.
        /// </summary>
        DrvSound = 0x00000009,

        /// <summary>
        /// File contains a system driver.
        /// </summary>
        DrvSystem = 0x00000007,

        /// <summary>
        /// File contains a versioned printer driver.
        /// </summary>
        DrvVersionedPrinter = 0x0000000C
    }
}