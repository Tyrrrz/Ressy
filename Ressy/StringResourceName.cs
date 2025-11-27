using System;
using Ressy.Native;

namespace Ressy;

internal partial class StringResourceName(string name) : ResourceName
{
    private readonly string _name = name;

    public override int? Code => null;

    public override string Label => _name;

    internal override NativeResource Marshal() => NativeMemory.Create(_name);
}

internal partial class StringResourceName : IEquatable<StringResourceName>
{
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
