using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ressy;

/// <summary>
/// Name of a resource stored in a portable executable image.
/// </summary>
public abstract partial class ResourceName
{
    /// <summary>
    /// Integer code that corresponds to the resource name.
    /// Can be null in case of a non-ordinal resource name.
    /// </summary>
    public abstract int? Code { get; }

    /// <summary>
    /// Resource name label in the format of "#69" (for ordinal names) or "MyResource" (for non-ordinal names).
    /// </summary>
    public abstract string Label { get; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => Label;
}

public partial class ResourceName
{
    private class OrdinalResourceName(int code) : ResourceName, IEquatable<OrdinalResourceName>
    {
        private readonly int _code = code;

        public override int? Code => _code;

        public override string Label => '#' + _code.ToString(CultureInfo.InvariantCulture);

        public bool Equals(OrdinalResourceName? other)
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

            return Equals((OrdinalResourceName)obj);
        }

        public override int GetHashCode() => _code;
    }

    private class StringResourceName(string name) : ResourceName, IEquatable<StringResourceName>
    {
        private readonly string _name = name;

        public override int? Code => null;

        public override string Label => _name;

        public bool Equals(StringResourceName? other)
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

            return Equals((StringResourceName)obj);
        }

        public override int GetHashCode() => _name.GetHashCode(StringComparison.Ordinal);
    }
}

public partial class ResourceName
{
    /// <summary>
    /// Creates an ordinal resource name from an integer code.
    /// </summary>
    public static ResourceName FromCode(int code) => new OrdinalResourceName(code);

    /// <summary>
    /// Creates a non-ordinal resource name from a string.
    /// </summary>
    public static ResourceName FromString(string name) => new StringResourceName(name);
}
