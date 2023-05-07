using System;
using Ressy.Native;

namespace Ressy;

internal partial class StringResourceName : ResourceName
{
    private readonly string _name;

    public override int? Code => null;

    public override string Label => _name;

    public StringResourceName(string name) => _name = name;

    internal override INativeHandle GetHandle() => NativeMemory.Create(_name);
}

internal partial class StringResourceName : IEquatable<StringResourceName>
{
    public bool Equals(StringResourceName? other)
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

        return Equals((StringResourceName)obj);
    }

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(_name);
}