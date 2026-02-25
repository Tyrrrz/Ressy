using System.Collections.Generic;

namespace Ressy.Icons;

// https://en.wikipedia.org/wiki/ICO_(file_format)#Outline
internal partial class IconGroup(IReadOnlyList<Icon> icons)
{
    public IReadOnlyList<Icon> Icons { get; } = icons;
}
