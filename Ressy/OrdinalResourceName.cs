using System;
using System.Globalization;
using Ressy.Native;

namespace Ressy
{
    internal partial class OrdinalResourceName : ResourceName
    {
        private readonly int _code;

        public override int? Code => _code;

        public override string Label => '#' + _code.ToString(CultureInfo.InvariantCulture);

        public OrdinalResourceName(int code) => _code = code;

        internal override SafeIntPtr ToPointer() => SafeIntPtr.FromValue(_code);
    }

    internal partial class OrdinalResourceName : IEquatable<OrdinalResourceName>
    {
        public bool Equals(OrdinalResourceName? other)
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

            return Equals((OrdinalResourceName)obj);
        }

        public override int GetHashCode() => _code;
    }
}