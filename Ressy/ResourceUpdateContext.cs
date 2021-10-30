using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Ressy.Native;

namespace Ressy
{
    public class ResourceUpdateContext
    {
        private readonly IntPtr _handle;

        internal ResourceUpdateContext(IntPtr handle) => _handle = handle;

        public void Set(ResourceDescriptor descriptor, byte[] data)
        {
            var dataHandle = Marshal.AllocHGlobal(data.Length);

            try
            {
                Marshal.Copy(data, 0, dataHandle, data.Length);

                if (!NativeMethods.UpdateResource(_handle,
                    descriptor.Type.Handle, descriptor.Name.Handle, descriptor.Language.Id,
                    dataHandle, (uint)data.Length))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(dataHandle);
            }
        }

        public void Remove(ResourceDescriptor descriptor)
        {
            if (!NativeMethods.UpdateResource(_handle,
                descriptor.Type.Handle, descriptor.Name.Handle, descriptor.Language.Id,
                IntPtr.Zero, 0))
            {
                throw new Win32Exception();
            }
        }

        internal void Commit(bool discardChanges = false)
        {
            if (!NativeMethods.EndUpdateResource(_handle, discardChanges))
                throw new Win32Exception();
        }
    }
}