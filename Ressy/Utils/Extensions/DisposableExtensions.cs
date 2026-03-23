using System;
using System.Collections.Generic;

namespace Ressy.Utils.Extensions;

internal static class DisposableExtensions
{
    extension(IEnumerable<IDisposable> source)
    {
        public void DisposeAll()
        {
            var exceptions = default(List<Exception>);

            foreach (var disposable in source)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            if (exceptions?.Count > 0)
                throw new AggregateException(exceptions);
        }
    }
}
