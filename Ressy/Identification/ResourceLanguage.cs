using System;

namespace Ressy.Identification
{
    public partial class ResourceLanguage
    {
        public ushort Id { get; }

        public ResourceLanguage(ushort id) => Id = id;

        public override string ToString() => Id.ToString();
    }

    public partial class ResourceLanguage
    {
        public static ResourceLanguage Neutral { get; } = new(0);

        public static ResourceLanguage EnglishUnitedStates { get; } = new(1033);
    }

    public partial class ResourceLanguage : IEquatable<ResourceLanguage>
    {
        public bool Equals(ResourceLanguage? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return obj is ResourceLanguage other && Equals(other);
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}