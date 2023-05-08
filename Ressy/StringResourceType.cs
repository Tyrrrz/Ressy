using System;
using Ressy.Native;

namespace Ressy;

internal partial class StringResourceType : ResourceType
{
    private readonly string _name;

    public override int? Code => null;

    public override string Label => _name;

    public StringResourceType(string name) => _name = name;

    internal override NativeResource Marshal() => NativeMemory.Create(_name);
}

internal partial class StringResourceType : IEquatable<StringResourceType>
{
    public bool Equals(StringResourceType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return StringComparer.Ordinal.Equals(_name, other._name);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((StringResourceType)obj);
    }

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(_name);
}