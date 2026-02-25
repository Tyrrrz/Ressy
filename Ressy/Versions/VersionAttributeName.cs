using System;

namespace Ressy.Versions;

/// <summary>
/// Encapsulates a name of an attribute stored inside a version info resource.
/// </summary>
public readonly partial struct VersionAttributeName(string raw)
{
    /// <summary>
    /// Raw attribute name.
    /// </summary>
    public string Raw { get; } = raw;

    /// <inheritdoc />
    public override string ToString() => Raw;
}

public partial struct VersionAttributeName
{
    /// <summary>
    /// Implicit conversion from string.
    /// </summary>
    public static implicit operator VersionAttributeName(string raw) => new(raw);

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    public static implicit operator string(VersionAttributeName name) => name.Raw;
}

// https://learn.microsoft.com/windows/win32/menurc/string-str#members
public partial struct VersionAttributeName
{
    /// <summary>
    /// Any additional information that should be displayed for diagnostic purposes.
    /// </summary>
    public static VersionAttributeName Comments => new("Comments");

    /// <summary>
    /// Identifies the company that produced the file.
    /// </summary>
    public static VersionAttributeName CompanyName => new("CompanyName");

    /// <summary>
    /// Describes the file in such a way that it can be presented to users.
    /// This string may be presented in a list box when the user is choosing files to install.
    /// For example, "Keyboard driver for AT-style keyboards" or "Microsoft Word for Windows".
    /// </summary>
    public static VersionAttributeName FileDescription => new("FileDescription");

    /// <summary>
    /// Identifies the version of this file.
    /// For example, this string could be "3.00A" or "5.00.RC2".
    /// </summary>
    public static VersionAttributeName FileVersion => new("FileVersion");

    /// <summary>
    /// Identifies the file's internal name, if one exists.
    /// For example, this string could contain the module name for a DLL, a virtual device name for a Windows
    /// virtual device, or a device name for a MS-DOS device driver.
    /// </summary>
    public static VersionAttributeName InternalName => new("InternalName");

    /// <summary>
    /// Describes all copyright notices, trademarks, and registered trademarks that apply to the file.
    /// This should include the full text of all notices, legal symbols, copyright dates, trademark numbers,
    /// and so on.
    /// In English, this string should be in the format "Copyright Microsoft Corp. 1990 1994".
    /// </summary>
    public static VersionAttributeName LegalCopyright => new("LegalCopyright");

    /// <summary>
    /// Describes all trademarks and registered trademarks that apply to the file.
    /// This should include the full text of all notices, legal symbols, trademark numbers, and so on.
    /// In English, this string should be in the format "Windows is a trademark of Microsoft Corporation".
    /// </summary>
    public static VersionAttributeName LegalTrademark => new("LegalTrademark");

    /// <summary>
    /// Identifies the original name of the file, not including a path.
    /// This enables an application to determine whether a file has been renamed by a user.
    /// This name may not be MS-DOS 8.3-format if the file is specific to a non-FAT file system.
    /// </summary>
    public static VersionAttributeName OriginalFilename => new("OriginalFilename");

    /// <summary>
    /// Describes by whom, where, and why this private version of the file was built.
    /// For example, this string could be "Built by OSCAR on \OSCAR2".
    /// </summary>
    public static VersionAttributeName PrivateBuild => new("PrivateBuild");

    /// <summary>
    /// Identifies the name of the product with which this file is distributed.
    /// For example, this string could be "Microsoft Windows".
    /// </summary>
    public static VersionAttributeName ProductName => new("ProductName");

    /// <summary>
    /// Identifies the version of the product with which this file is distributed.
    /// For example, this string could be "3.00A" or "5.00.RC2".
    /// </summary>
    public static VersionAttributeName ProductVersion => new("ProductVersion");

    /// <summary>
    /// Describes how this version of the file differs from the normal version.
    /// For example, this string could be
    /// "Private build for Olivetti solving mouse problems on M250 and M250E computers".
    /// </summary>
    public static VersionAttributeName SpecialBuild => new("SpecialBuild");
}

public partial struct VersionAttributeName : IEquatable<VersionAttributeName>
{
    /// <inheritdoc />
    public bool Equals(VersionAttributeName other) =>
        string.Equals(Raw, other.Raw, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is VersionAttributeName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Raw.GetHashCode(StringComparison.Ordinal);
}
