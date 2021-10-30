using System;
using Ressy.Native;
using Ressy.Utils;

namespace Ressy.Identification
{
    public abstract partial class ResourceName
    {
        public abstract int? Code { get; }

        public abstract string Label { get; }

        internal abstract IUnmanagedMemory CreateMemory();

        public override string ToString() => Label;
    }

    public partial class ResourceName
    {
        public static ResourceName FromCode(int code) => new OrdinalResourceName(code);

        public static ResourceName FromString(string name) => new StringResourceName(name);

        internal static ResourceName FromHandle(IntPtr handle) => NativeHelpers.IsIntegerCode(handle)
            ? FromCode(handle.ToInt32())
            : FromString(NativeHelpers.GetString(handle));
    }
}