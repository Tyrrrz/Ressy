using System;
using Ressy.Native;
using Ressy.Utils;

namespace Ressy.Identification
{
    public abstract partial class ResourceType
    {
        public abstract int? Code { get; }

        public abstract string Label { get; }

        internal abstract IUnmanagedMemory CreateMemory();

        public override string ToString() => Label;

    }

    public partial class ResourceType
    {
        public static ResourceType FromCode(int code) => new OrdinalResourceType(code);

        public static ResourceType FromCode(StandardResourceTypeCode code) => FromCode((int)code);

        public static ResourceType FromString(string name) => new StringResourceType(name);

        internal static ResourceType FromHandle(IntPtr handle) => NativeHelpers.IsIntegerCode(handle)
            ? FromCode(handle.ToInt32())
            : FromString(NativeHelpers.GetString(handle));
    }
}