using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Ressy.Native;

namespace Ressy
{
    public class Resource
    {
        public IntPtr Handle { get; }

        public PortableExecutable Module { get; }

        public ResourceType Type { get; }

        public ResourceName Name { get; }

        public ResourceLanguage Language { get; }

        public Resource(
            IntPtr handle,
            PortableExecutable module,
            ResourceType type,
            ResourceName name,
            ResourceLanguage language)
        {
            Handle = handle;
            Module = module;
            Type = type;
            Name = name;
            Language = language;
        }

        public byte[] GetData()
        {
            var length = NativeMethods.SizeofResource(Module.Handle, Handle);
            if (length <= 0)
                throw new Win32Exception();

            var dataHandle = NativeMethods.LoadResource(Module.Handle, Handle);
            if (dataHandle == IntPtr.Zero)
                throw new Win32Exception();

            var dataSource = NativeMethods.LockResource(dataHandle);
            if (dataSource == IntPtr.Zero)
                throw new Win32Exception();

            var data = new byte[length];
            Marshal.Copy(dataSource, data, 0, (int)length);

            return data;
        }

        public override string ToString() => $"[{Type}] {Name} (Lang: {Language})";
    }
}