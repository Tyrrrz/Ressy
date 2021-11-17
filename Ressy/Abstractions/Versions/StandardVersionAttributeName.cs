namespace Ressy.Abstractions.Versions
{
    /// <summary>
    /// Defines names for standard version attributes.
    /// </summary>
    // https://docs.microsoft.com/en-us/windows/win32/menurc/string-str#members
    public enum StandardVersionAttributeName
    {
        /// <summary>
        /// Any additional information that should be displayed for diagnostic purposes.
        /// </summary>
        Comments,

        /// <summary>
        /// Identifies the company that produced the file.
        /// </summary>
        CompanyName,

        /// <summary>
        /// Describes the file in such a way that it can be presented to users.
        /// This string may be presented in a list box when the user is choosing files to install.
        /// For example, "Keyboard driver for AT-style keyboards" or "Microsoft Word for Windows".
        /// </summary>
        FileDescription,

        /// <summary>
        /// Identifies the version of this file.
        /// For example, this string could be "3.00A" or "5.00.RC2".
        /// </summary>
        FileVersion,

        /// <summary>
        /// Identifies the file's internal name, if one exists.
        /// For example, this string could contain the module name for a DLL, a virtual device name for a Windows
        /// virtual device, or a device name for a MS-DOS device driver.
        /// </summary>
        InternalName,

        /// <summary>
        /// Describes all copyright notices, trademarks, and registered trademarks that apply to the file.
        /// This should include the full text of all notices, legal symbols, copyright dates, trademark numbers,
        /// and so on.
        /// In English, this string should be in the format "Copyright Microsoft Corp. 1990 1994".
        /// </summary>
        LegalCopyright,

        /// <summary>
        /// Describes all trademarks and registered trademarks that apply to the file.
        /// This should include the full text of all notices, legal symbols, trademark numbers, and so on.
        /// In English, this string should be in the format "Windows is a trademark of Microsoft Corporation".
        /// </summary>
        LegalTrademark,

        /// <summary>
        /// Identifies the original name of the file, not including a path.
        /// This enables an application to determine whether a file has been renamed by a user.
        /// This name may not be MS-DOS 8.3-format if the file is specific to a non-FAT file system.
        /// </summary>
        OriginalFilename,

        /// <summary>
        /// Describes by whom, where, and why this private version of the file was built.
        /// For example, this string could be "Built by OSCAR on \OSCAR2".
        /// </summary>
        PrivateBuild,

        /// <summary>
        /// Identifies the name of the product with which this file is distributed.
        /// For example, this string could be "Microsoft Windows".
        /// </summary>
        ProductName,

        /// <summary>
        /// Identifies the version of the product with which this file is distributed.
        /// For example, this string could be "3.00A" or "5.00.RC2".
        /// </summary>
        ProductVersion,

        /// <summary>
        /// Describes how this version of the file differs from the normal version.
        /// For example, this string could be
        /// "Private build for Olivetti solving mouse problems on M250 and M250E computers".
        /// </summary>
        SpecialBuild
    }
}