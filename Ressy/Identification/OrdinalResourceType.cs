using System;
using System.Globalization;
using Ressy.Utils;

namespace Ressy.Identification
{
    internal partial class OrdinalResourceType : ResourceType
    {
        private readonly int _code;

        public override int? Code => _code;

        public override string Label => (StandardResourceTypeCode)_code switch
        {
            StandardResourceTypeCode.Cursor => "CURSOR",
            StandardResourceTypeCode.Bitmap => "BITMAP",
            StandardResourceTypeCode.Icon => "ICON",
            StandardResourceTypeCode.Menu => "MENU",
            StandardResourceTypeCode.Dialog => "DIALOG",
            StandardResourceTypeCode.String => "STRING",
            StandardResourceTypeCode.FontDir => "FONTDIR",
            StandardResourceTypeCode.Font => "FONT",
            StandardResourceTypeCode.Accelerator => "ACCELERATOR",
            StandardResourceTypeCode.RawData => "RCDATA",
            StandardResourceTypeCode.MessageTable => "MESSAGETABLE",
            StandardResourceTypeCode.GroupCursor => "GROUP_CURSOR",
            StandardResourceTypeCode.GroupIcon => "GROUP_ICON",
            StandardResourceTypeCode.Version => "VERSION",
            StandardResourceTypeCode.DlgInclude => "DLGINCLUDE",
            StandardResourceTypeCode.PlugAndPlay => "PLUGPLAY",
            StandardResourceTypeCode.Vxd => "VXD",
            StandardResourceTypeCode.AnimatedCursor => "ANICURSOR",
            StandardResourceTypeCode.AnimatedIcon => "ANIICON",
            StandardResourceTypeCode.Html => "HTML",
            StandardResourceTypeCode.Manifest => "MANIFEST",
            _ => '#' + _code.ToString(CultureInfo.InvariantCulture)
        };

        public OrdinalResourceType(int code) => _code = code;

        internal override IUnmanagedMemory CreateMemory() => new PreallocatedUnmanagedMemory(_code);
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
}