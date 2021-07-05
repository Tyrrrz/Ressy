using System;
using System.Globalization;
using Ressy.Native;

namespace Ressy
{
    public partial class ResourceType
    {
        public string Identifier { get; }

        public string? FriendlyName { get; }

        public ResourceType(string identifier, string? friendlyName = null)
        {
            Identifier = identifier;
            FriendlyName = friendlyName;
        }

        public override string ToString() => FriendlyName ?? Identifier;
    }

    public enum StandardResourceTypeCode
    {
        Cursor = 1,
        Bitmap = 2,
        Icon = 3,
        Menu = 4,
        Dialog = 5,
        String = 6,
        FontDir = 7,
        Font = 8,
        Accelerator = 9,
        RawData = 10,
        MessageTable = 11,
        GroupCursor = 12,
        GroupIcon = 14,
        Version = 16,
        DlgInclude = 17,
        PlugAndPlay = 19,
        Vxd = 20,
        AnimatedCursor = 21,
        AnimatedIcon = 22,
        Html = 23,
        Manifest = 24
    }

    public partial class ResourceType
    {
        public static ResourceType FromCode(StandardResourceTypeCode code)
        {
            var codeNumeric = (int) code;

            var identifier = '#' + codeNumeric.ToString(CultureInfo.InvariantCulture);

            var friendlyName = code switch
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
                _ => null
            };

            return new ResourceType(identifier, friendlyName);
        }

        public static ResourceType FromCode(int code) => FromCode((StandardResourceTypeCode) code);

        public static ResourceType FromString(string type) =>
            type.StartsWith('#') &&
            int.TryParse(type.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var code)
                ? FromCode(code)
                : new ResourceType(type);

        public static ResourceType FromHandle(IntPtr handle)
        {
            return NativeHelpers.IsIntegerCode(handle)
                ? FromCode(handle.ToInt32())
                : FromString(NativeHelpers.GetString(handle));
        }
    }

    public partial class ResourceType : IEquatable<ResourceType>
    {
        public bool Equals(ResourceType? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(Identifier, other.Identifier, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((ResourceType) obj);
        }

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Identifier);

        public static bool operator ==(ResourceType? a, ResourceType? b) => a?.Equals(b) ?? b is null;

        public static bool operator !=(ResourceType? a, ResourceType? b) => !(a == b);
    }
}