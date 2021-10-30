using System;
using Ressy.Native;
using Ressy.Utils;

namespace Ressy.Identification
{
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

        internal abstract IUnmanagedMemory CreateMemory();

        /// <inheritdoc />
        public override string ToString() => Label;
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

        internal static ResourceName FromHandle(IntPtr handle) => NativeHelpers.IsIntegerCode(handle)
            ? FromCode(handle.ToInt32())
            : FromString(NativeHelpers.GetString(handle));
    }
}