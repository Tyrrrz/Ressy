using System;
using System.Diagnostics.CodeAnalysis;
using Ressy.Native;

namespace Ressy
{
    /// <summary>
    /// Type of a resource stored in a portable executable image.
    /// </summary>
    public abstract partial class ResourceType
    {
        /// <summary>
        /// Integer code that corresponds to the resource type.
        /// Can be null in case of a non-ordinal resource type.
        /// </summary>
        public abstract int? Code { get; }

        /// <summary>
        /// Resource type label in the format of "#14 (GROUP_ICON)" (for standard ordinal types) or
        /// "#69" (for non-standard ordinal types) or "MyResource" (for non-ordinal types).
        /// </summary>
        public abstract string Label { get; }

        /// <summary>
        /// Marshals the value of this resource type to native memory for use with Windows API.
        /// </summary>
        internal abstract SafeIntPtr ToPointer();

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => Label;
    }

    public partial class ResourceType
    {
        /// <summary>
        /// Creates an ordinal resource type from an integer code.
        /// </summary>
        public static ResourceType FromCode(int code) => new OrdinalResourceType(code);

        /// <summary>
        /// Creates a standard ordinal resource type from an integer code.
        /// </summary>
        public static ResourceType FromCode(StandardResourceTypeCode code) => FromCode((int)code);

        /// <summary>
        /// Creates a non-ordinal resource type from a string.
        /// </summary>
        public static ResourceType FromString(string type) => new StringResourceType(type);

        internal static ResourceType FromHandle(IntPtr handle) => handle.ToInt64() < 0x10000
            ? FromCode(handle.ToInt32())
            : FromString(NativeHelpers.GetString(handle));
    }
}