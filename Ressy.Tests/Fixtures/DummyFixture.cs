using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ressy.Tests.Utils;

namespace Ressy.Tests.Fixtures
{
    public class DummyFixture : IDisposable
    {
        private readonly ConcurrentBag<string> _filePaths = new();

        public string CreatePortableExecutable()
        {
            var sourceFilePath = Path.ChangeExtension(typeof(Dummy.Program).Assembly.Location, "exe");
            var destFilePath = Path.Combine(DirectoryEx.ExecutingDirectoryPath, $"Test-{Guid.NewGuid()}.tmp");

            File.Copy(sourceFilePath, destFilePath);
            _filePaths.Add(destFilePath);

            return destFilePath;
        }

        public void Dispose()
        {
            var exceptions = new List<Exception>();

            foreach (var filePath in _filePaths)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (FileNotFoundException)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }
    }
}