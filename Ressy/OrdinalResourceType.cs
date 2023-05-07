using System;
using System.Globalization;
using Ressy.Native;

namespace Ressy;

internal partial class OrdinalResourceType : ResourceType
{
    private readonly int _code;

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
                _ => null
            };

            return !string.IsNullOrWhiteSpace(standardTypePortion)
                ? codePortion + ' ' + '(' + standardTypePortion + ')'
                : codePortion;
        }
    }

    public OrdinalResourceType(int code) => _code = code;

    internal override INativeHandle GetHandle() => new NoopNativeHandle(new IntPtr(_code));
}

internal partial class OrdinalResourceType : IEquatable<OrdinalResourceType>
{
    public bool Equals(OrdinalResourceType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return _code == other._code;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((OrdinalResourceType)obj);
    }

    public override int GetHashCode() => _code;
}