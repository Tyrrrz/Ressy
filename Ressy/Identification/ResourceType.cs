﻿using System;
using Ressy.Native;
using Ressy.Utils;

namespace Ressy.Identification
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
        /// Resource type label in the format of "GROUP_ICON" (for standard ordinal types) or
        /// "#69" (for custom ordinal types) or "MyResource" (for non-ordinal types).
        /// </summary>
        public abstract string Label { get; }

        internal abstract IUnmanagedMemory CreateMemory();

        /// <inheritdoc />
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

        internal static ResourceType FromHandle(IntPtr handle) => NativeHelpers.IsIntegerCode(handle)
            ? FromCode(handle.ToInt32())
            : FromString(NativeHelpers.GetString(handle));
    }
}