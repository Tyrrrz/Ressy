using System;

namespace Ressy.Utils.Extensions;

internal static class VersionExtensions
{
    // .NET versions can have components set to -1 which indicates that they are not used.
    // E.g. new Version(1, 2) creates a version with components 1, 2, -1, -1.
    // Sane version representations are not supposed to do that, so we need a way to correct such cases.
    public static Version ClampComponents(this Version version)
    {
        var major = Math.Max(0, version.Major);
        var minor = Math.Max(0, version.Minor);
        var build = Math.Max(0, version.Build);
        var revision = Math.Max(0, version.Revision);

        return
            major != version.Major
            || minor != version.Minor
            || build != version.Build
            || revision != version.Revision
            ? new Version(major, minor, build, revision)
            : version;
    }
}
