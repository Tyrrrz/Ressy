namespace Ressy
{
    /// <summary>
    /// Defines codes for standard ordinal resource types.
    /// </summary>
    // https://docs.microsoft.com/en-us/windows/win32/menurc/resource-types
    public enum StandardResourceTypeCode
    {
        /// <summary>
        /// Corresponds to "RT_CURSOR".
        /// </summary>
        Cursor = 1,

        /// <summary>
        /// Corresponds to "RT_BITMAP".
        /// </summary>
        Bitmap = 2,

        /// <summary>
        /// Corresponds to "RT_ICON".
        /// </summary>
        Icon = 3,

        /// <summary>
        /// Corresponds to "RT_MENU".
        /// </summary>
        Menu = 4,

        /// <summary>
        /// Corresponds to "RT_DIALOG".
        /// </summary>
        Dialog = 5,

        /// <summary>
        /// Corresponds to "RT_STRING".
        /// </summary>
        String = 6,

        /// <summary>
        /// Corresponds to "RT_FONTDIR".
        /// </summary>
        FontDir = 7,

        /// <summary>
        /// Corresponds to "RT_FONT".
        /// </summary>
        Font = 8,

        /// <summary>
        /// Corresponds to "RT_ACCELERATOR".
        /// </summary>
        Accelerator = 9,

        /// <summary>
        /// Corresponds to "RT_RCDATA".
        /// </summary>
        RawData = 10,

        /// <summary>
        /// Corresponds to "RT_MESSAGETABLE".
        /// </summary>
        MessageTable = 11,

        /// <summary>
        /// Corresponds to "RT_GROUP_CURSOR".
        /// </summary>
        GroupCursor = 12,

        /// <summary>
        /// Corresponds to "RT_GROUP_ICON".
        /// </summary>
        GroupIcon = 14,

        /// <summary>
        /// Corresponds to "RT_VERSION".
        /// </summary>
        Version = 16,

        /// <summary>
        /// Corresponds to "RT_DLGINCLUDE".
        /// </summary>
        DlgInclude = 17,

        /// <summary>
        /// Corresponds to "RT_PLUGPLAY".
        /// </summary>
        PlugAndPlay = 19,

        /// <summary>
        /// Corresponds to "RT_VXD".
        /// </summary>
        Vxd = 20,

        /// <summary>
        /// Corresponds to "RT_ANICURSOR".
        /// </summary>
        AnimatedCursor = 21,

        /// <summary>
        /// Corresponds to "RT_ANIICON".
        /// </summary>
        AnimatedIcon = 22,

        /// <summary>
        /// Corresponds to "RT_HTML".
        /// </summary>
        Html = 23,

        /// <summary>
        /// Corresponds to "RT_MANIFEST".
        /// </summary>
        Manifest = 24
    }
}