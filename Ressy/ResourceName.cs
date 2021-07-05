using System.Globalization;
using System;
using Ressy.Native;

namespace Ressy
{
    public partial class ResourceName
    {
        public string Identifier { get; }

        public ResourceName(string identifier) => Identifier = identifier;

        public override string ToString() => Identifier;
    }

    public partial class ResourceName
    {
        public static ResourceName FromCode(int code)
        {
            var identifier = '#' + code.ToString(CultureInfo.InvariantCulture);
            return new ResourceName(identifier);
        }

        public static ResourceName FromString(string name) =>
            name.StartsWith('#') &&
            int.TryParse(name.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var code)
                ? FromCode(code)
                : new ResourceName(name);

        public static ResourceName FromHandle(IntPtr handle) =>
            NativeHelpers.IsIntegerCode(handle)
                ? FromCode(handle.ToInt32())
                : FromString(NativeHelpers.GetString(handle));
    }

    public partial class ResourceName : IEquatable<ResourceName>
    {
        public bool Equals(ResourceName? other)
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

            return Equals((ResourceName) obj);
        }

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Identifier);

        public static bool operator ==(ResourceName? a, ResourceName? b) => a?.Equals(b) ?? b is null;

        public static bool operator !=(ResourceName? a, ResourceName? b) => !(a == b);
    }
}