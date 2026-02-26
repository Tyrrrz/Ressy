using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ressy;

/// <summary>
/// Type of a resource stored in a portable executable image.
/// </summary>
public abstract partial class ResourceType
{
    /// <summary>
    /// Integer code that corresponds to the resource type.
    /// Can be null in case of a non-ordinal resource type.
    /// </summary>
    public abstract int? Code { get; }

    /// <summary>
    /// Resource type label in the format of "#14 (GROUP_ICON)" (for standard ordinal types) or
    /// "#69" (for non-standard ordinal types) or "MyResource" (for non-ordinal types).
    /// </summary>
    public abstract string Label { get; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => Label;
}

public partial class ResourceType
{
    private class OrdinalResourceType(int code) : ResourceType, IEquatable<OrdinalResourceType>
    {
        private readonly int _code = code;

        public override int? Code => _code;

        public override string Label
        {
            get
            {
                var codePortion = '#' + _code.ToString(CultureInfo.InvariantCulture);

                var standardTypePortion = _code switch
                {
                    1 => "CURSOR",
                    2 => "BITMAP",
                    3 => "ICON",
                    4 => "MENU",
                    5 => "DIALOG",
                    6 => "STRING",
                    7 => "FONTDIR",
                    8 => "FONT",
                    9 => "ACCELERATOR",
                    10 => "RCDATA",
                    11 => "MESSAGETABLE",
                    12 => "GROUP_CURSOR",
                    14 => "GROUP_ICON",
                    16 => "VERSION",
                    17 => "DLGINCLUDE",
                    19 => "PLUGPLAY",
                    20 => "VXD",
                    21 => "ANICURSOR",
                    22 => "ANIICON",
                    23 => "HTML",
                    24 => "MANIFEST",
                    _ => null,
                };

                return !string.IsNullOrWhiteSpace(standardTypePortion)
                    ? codePortion + ' ' + '(' + standardTypePortion + ')'
                    : codePortion;
            }
        }

        public bool Equals(OrdinalResourceType? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return _code == other._code;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;

            return Equals((OrdinalResourceType)obj);
        }

        public override int GetHashCode() => _code;
    }

    private class StringResourceType(string name) : ResourceType, IEquatable<StringResourceType>
    {
        private readonly string _name = name;

        public override int? Code => null;

        public override string Label => _name;

        public bool Equals(StringResourceType? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(_name, other._name, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;

            return Equals((StringResourceType)obj);
        }

        public override int GetHashCode() => _name.GetHashCode(StringComparison.Ordinal);
    }
}

public partial class ResourceType
{
    /// <summary>
    /// Creates an ordinal resource type from an integer code.
    /// </summary>
    public static ResourceType FromCode(int code) => new OrdinalResourceType(code);

    /// <summary>
    /// Creates a non-ordinal resource type from a string.
    /// </summary>
    public static ResourceType FromString(string type) => new StringResourceType(type);
}

// https://learn.microsoft.com/windows/win32/menurc/resource-types
public partial class ResourceType
{
    /// <summary>
    /// Corresponds to RT_CURSOR.
    /// </summary>
    public static ResourceType Cursor { get; } = FromCode(1);

    /// <summary>
    /// Corresponds to RT_BITMAP.
    /// </summary>
    public static ResourceType Bitmap { get; } = FromCode(2);

    /// <summary>
    /// Corresponds to RT_ICON.
    /// </summary>
    public static ResourceType Icon { get; } = FromCode(3);

    /// <summary>
    /// Corresponds to RT_MENU.
    /// </summary>
    public static ResourceType Menu { get; } = FromCode(4);

    /// <summary>
    /// Corresponds to RT_DIALOG.
    /// </summary>
    public static ResourceType Dialog { get; } = FromCode(5);

    /// <summary>
    /// Corresponds to RT_STRING.
    /// </summary>
    public static ResourceType String { get; } = FromCode(6);

    /// <summary>
    /// Corresponds to RT_FONTDIR.
    /// </summary>
    public static ResourceType FontDir { get; } = FromCode(7);

    /// <summary>
    /// Corresponds to RT_FONT.
    /// </summary>
    public static ResourceType Font { get; } = FromCode(8);

    /// <summary>
    /// Corresponds to RT_ACCELERATOR.
    /// </summary>
    public static ResourceType Accelerator { get; } = FromCode(9);

    /// <summary>
    /// Corresponds to RT_RCDATA.
    /// </summary>
    public static ResourceType RawData { get; } = FromCode(10);

    /// <summary>
    /// Corresponds to RT_MESSAGETABLE.
    /// </summary>
    public static ResourceType MessageTable { get; } = FromCode(11);

    /// <summary>
    /// Corresponds to RT_GROUP_CURSOR.
    /// </summary>
    public static ResourceType CursorGroup { get; } = FromCode(12);

    /// <summary>
    /// Corresponds to RT_GROUP_ICON.
    /// </summary>
    public static ResourceType IconGroup { get; } = FromCode(14);

    /// <summary>
    /// Corresponds to RT_VERSION.
    /// </summary>
    public static ResourceType Version { get; } = FromCode(16);

    /// <summary>
    /// Corresponds to RT_DLGINCLUDE.
    /// </summary>
    public static ResourceType DialogInclude { get; } = FromCode(17);

    /// <summary>
    /// Corresponds to RT_PLUGPLAY.
    /// </summary>
    public static ResourceType PlugAndPlay { get; } = FromCode(19);

    /// <summary>
    /// Corresponds to RT_VXD.
    /// </summary>
    public static ResourceType Vxd { get; } = FromCode(20);

    /// <summary>
    /// Corresponds to RT_ANICURSOR.
    /// </summary>
    public static ResourceType AnimatedCursor { get; } = FromCode(21);

    /// <summary>
    /// Corresponds to RT_ANIICON.
    /// </summary>
    public static ResourceType AnimatedIcon { get; } = FromCode(22);

    /// <summary>
    /// Corresponds to RT_HTML.
    /// </summary>
    public static ResourceType Html { get; } = FromCode(23);

    /// <summary>
    /// Corresponds to RT_MANIFEST.
    /// </summary>
    public static ResourceType Manifest { get; } = FromCode(24);

    /// <summary>
    /// Corresponds to "MUI".
    /// </summary>
    // https://learn.microsoft.com/windows/win32/intl/mui-resource-technology
    public static ResourceType Mui { get; } = FromString("\"MUI\"");
}
