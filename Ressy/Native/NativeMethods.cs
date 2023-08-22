using System.Runtime.InteropServices;

namespace Ressy.Native;

internal static class NativeMethods
{
    private const string Kernel32 = "kernel32.dll";

    [DllImport(Kernel32, SetLastError = true)]
    public static extern nint LoadLibraryEx(string lpFileName, nint hFile, uint dwFlags);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool FreeLibrary(nint hModule);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumResourceTypesEx(
        nint hModule,
        EnumResTypeProc lpEnumFunc,
        nint lParam,
        uint dwFlags,
        ushort langId
    );

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumResourceNamesEx(
        nint hModule,
        nint lpType,
        EnumResNameProc lpEnumFunc,
        nint lParam,
        uint dwFlags,
        ushort langId
    );

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumResourceLanguagesEx(
        nint hModule,
        nint lpType,
        nint lpName,
        EnumResLangProc lpEnumFunc,
        nint lParam,
        uint dwFlags,
        ushort langId
    );

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint FindResourceEx(
        nint hModule,
        nint lpType,
        nint lpName,
        ushort wLanguage
    );

    [DllImport(Kernel32, SetLastError = true)]
    public static extern uint SizeofResource(nint hModule, nint hResInfo);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern nint LoadResource(nint hModule, nint hResInfo);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern nint LockResource(nint hResData);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool EndUpdateResource(nint hUpdate, bool fDiscard);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool UpdateResource(
        nint hUpdate,
        nint lpType,
        nint lpName,
        ushort wLanguage,
        nint lpData,
        uint cbData
    );
}
