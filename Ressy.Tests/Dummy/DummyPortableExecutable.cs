using System;
using System.Globalization;
using System.IO;

namespace Ressy.Tests.Dummy
{
    public partial class DummyPortableExecutable : IDisposable
    {
        public string FilePath { get; }

        public DummyPortableExecutable(string filePath) => FilePath = filePath;

        public void Dispose() => File.Delete(FilePath);
    }

    public partial class DummyPortableExecutable
    {
        private static string ExecutingDirPath { get; } =
            Path.GetDirectoryName(typeof(DummyPortableExecutable).Assembly.Location) ??
            Directory.GetCurrentDirectory();

        public static DummyPortableExecutable Create(string directoryPath)
        {
            var sourceFilePath = Path.Combine(ExecutingDirPath, "SmallPEpe.exe");

            var suffix = Math.Abs(Guid.NewGuid().GetHashCode()).ToString(CultureInfo.InvariantCulture);
            var filePath = Path.Combine(directoryPath, $"PE-{suffix}.exe");

            File.Copy(sourceFilePath, filePath);
            return new DummyPortableExecutable(filePath);
        }

        public static DummyPortableExecutable Create() => Create(ExecutingDirPath);
    }
}