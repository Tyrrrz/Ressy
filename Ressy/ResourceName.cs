using System.Globalization;
using System;
using Ressy.Native;

namespace Ressy
{
    public partial class ResourceName
    {
        internal IntPtr Handle { get; }

        public string Identifier => NativeHelpers.IsIntegerCode(Handle)
            ? '#' + Handle.ToInt32().ToString(CultureInfo.InvariantCulture)
            : NativeHelpers.GetString(Handle);

        internal ResourceName(IntPtr handle) => Handle = handle;

        public override string ToString() => Identifier;
    }

    public partial class ResourceName
    {
        public static ResourceName FromCode(int code) => new(new IntPtr(code));

        public static ResourceName FromString(string name) => throw new NotImplementedException();
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