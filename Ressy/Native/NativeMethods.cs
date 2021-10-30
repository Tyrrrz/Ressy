using System;
using System.Runtime.InteropServices;

namespace Ressy.Native
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumResourceTypesEx(
            IntPtr hModule,
            EnumResTypeProc lpEnumFunc,
            IntPtr lParam,
            uint dwFlags,
            ushort langId
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumResourceNamesEx(
            IntPtr hModule,
            IntPtr lpType,
            EnumResNameProc lpEnumFunc,
            IntPtr lParam,
            uint dwFlags,
            ushort langId
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumResourceLanguagesEx(
            IntPtr hModule,
            IntPtr lpType,
            IntPtr lpName,
            EnumResLangProc lpEnumFunc,
            IntPtr lParam,
            uint dwFlags,
            ushort langId
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindResourceEx(
            IntPtr hModule,
            IntPtr lpType,
            IntPtr lpName,
            ushort wLanguage
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UpdateResource(
            IntPtr hUpdate,
            IntPtr lpType,
            IntPtr lpName,
            ushort wLanguage,
            IntPtr lpData,
            uint cbData
        );
    }
}