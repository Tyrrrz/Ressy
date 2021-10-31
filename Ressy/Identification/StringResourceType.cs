using Ressy.Utils;

namespace Ressy.Identification
{
    internal class StringResourceType : ResourceType
    {
        private readonly string _name;

        public override int? Code => null;

        public override string Label => _name;

        public StringResourceType(string name) => _name = name;

        internal override IUnmanagedMemory ToUnmanagedMemory() =>
            new StringUnmanagedMemory(_name);
    }
}