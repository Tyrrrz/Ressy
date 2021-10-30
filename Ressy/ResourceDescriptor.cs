namespace Ressy
{
    public class ResourceDescriptor
    {
        public ResourceType Type { get; }

        public ResourceName Name { get; }

        public ResourceLanguage Language { get; }

        public ResourceDescriptor(ResourceType type, ResourceName name, ResourceLanguage language)
        {
            Type = type;
            Name = name;
            Language = language;
        }

        public ResourceDescriptor(ResourceType type, ResourceName name)
            : this(type, name, ResourceLanguage.Neutral)
        {
        }

        public override string ToString() => $"{Type} / {Name} / {Language}";
    }
}