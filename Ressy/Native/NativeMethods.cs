using System;
using System.Runtime.InteropServices;

namespace Ressy.Native;

internal static class NativeMethods
{
    private const string Kernel32 = "kernel32.dll";

    [DllImport(Kernel32, SetLastError = true)]
    public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool FreeLibrary(IntPtr hModule);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumResourceTypesEx(
        IntPtr hModule,
        EnumResTypeProc lpEnumFunc,
        IntPtr lParam,
        uint dwFlags,
        ushort langId
    );

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumResourceNamesEx(
        IntPtr hModule,
        IntPtr lpType,
        EnumResNameProc lpEnumFunc,
        IntPtr lParam,
        uint dwFlags,
        ushort langId
    );

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumResourceLanguagesEx(
        IntPtr hModule,
        IntPtr lpType,
        IntPtr lpName,
        EnumResLangProc lpEnumFunc,
        IntPtr lParam,
        uint dwFlags,
        ushort langId
    );

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr FindResourceEx(
        IntPtr hModule,
        IntPtr lpType,
        IntPtr lpName,
        ushort wLanguage
    );

    [DllImport(Kernel32, SetLastError = true)]
    public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern IntPtr LockResource(IntPtr hResData);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool UpdateResource(
        IntPtr hUpdate,
        IntPtr lpType,
        IntPtr lpName,
        ushort wLanguage,
        IntPtr lpData,
        uint cbData
    );
}