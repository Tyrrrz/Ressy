using System;
using Xunit;

namespace Ressy.Tests.Utils;

/// <summary>
/// Marks a test that should only run on Windows.
/// </summary>
public class WindowsOnlyFactAttribute : FactAttribute
{
    public WindowsOnlyFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
            Skip = "Only supported on Windows.";
    }
}
