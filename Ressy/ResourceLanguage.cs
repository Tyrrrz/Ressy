using System;

namespace Ressy
{
    public partial struct ResourceLanguage
    {
        public ushort Id { get; }

        public ResourceLanguage(ushort id) => Id = id;

        public override string ToString() => Id.ToString();
    }

    public partial struct ResourceLanguage
    {
        public static ResourceLanguage Neutral => new(0);

        public static ResourceLanguage EnglishUnitedStates => new(1033);
    }

    public partial struct ResourceLanguage : IEquatable<ResourceLanguage>
    {
        public bool Equals(ResourceLanguage other) => Id == other.Id;

        public override bool Equals(object? obj) => obj is ResourceLanguage other && Equals(other);

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(ResourceLanguage a, ResourceLanguage b) => a.Equals(b);

        public static bool operator !=(ResourceLanguage a, ResourceLanguage b) => !(a == b);
    }
}