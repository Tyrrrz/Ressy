using System;
using System.Globalization;
using System.IO;
using Ressy.MultilingualUserInterface;

namespace Ressy.Tests.Utils;

// Creates a language-specific satellite .mui file alongside a MUI-enabled neutral PE so that
// Windows' FileVersionInfo API (which respects MUI redirection) can read version strings
// from the satellite rather than failing to find one.
internal sealed class TempMuiSatellite : IDisposable
{
    private readonly string _dirPath;

    private TempMuiSatellite(string dirPath) => _dirPath = dirPath;

    public void Dispose()
    {
        try
        {
            Directory.Delete(_dirPath, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }

    // Creates a satellite for the given neutral PE under <dir>/<lang>/<filename>.mui.
    // The satellite is a copy of the neutral file whose MUI resource is replaced with
    // a LanguageSpecific one so Windows doesn't try to recurse into another satellite.
    public static TempMuiSatellite Create(string neutralFilePath)
    {
        var lang = CultureInfo.CurrentUICulture.Name;
        var dir = Path.Combine(Path.GetDirectoryName(neutralFilePath)!, lang);
        Directory.CreateDirectory(dir);

        var satellitePath = Path.Combine(dir, Path.GetFileName(neutralFilePath) + ".mui");
        File.Copy(neutralFilePath, satellitePath, overwrite: true);

        // Replace the inherited LanguageNeutral MUI with a LanguageSpecific one so that
        // Windows does not try to load a second-level satellite from this file.
        using var pe = PortableExecutable.OpenWrite(satellitePath);
        pe.SetMuiInfo(
            new MuiInfo(
                MuiFileType.LanguageSpecific,
                checksum: new byte[16],
                serviceChecksum: new byte[16],
                mainResourceTypes: [],
                fallbackResourceTypes: [],
                language: lang,
                fallbackLanguage: lang,
                ultimateFallbackLanguage: null
            )
        );

        return new TempMuiSatellite(dir);
    }
}
