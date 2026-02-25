using System.Collections.Generic;

namespace Ressy.Utils.Extensions;

internal static class CollectionExtensions
{
    extension<T>(IEnumerable<T?> source)
        where T : struct
    {
        public IEnumerable<T> WhereNotNull()
        {
            foreach (var i in source)
            {
                if (i is not null)
                    yield return i.Value;
            }
        }
    }
}
