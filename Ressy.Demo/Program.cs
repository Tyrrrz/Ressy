using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Ressy.Identification;

namespace Ressy.Demo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var imageFilePath = args.ElementAtOrDefault(0);
            if (string.IsNullOrWhiteSpace(imageFilePath) || string.Equals(imageFilePath, "-", StringComparison.Ordinal))
                imageFilePath = typeof(Program).Assembly.Location;

            var resourceType =
                args.ElementAtOrDefault(1) is { } typeString
                    ? int.TryParse(typeString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var typeCode)
                        ? ResourceType.FromCode(typeCode)
                        : ResourceType.FromString(typeString)
                    : null;

            var resourceName = args.ElementAtOrDefault(2) is { } nameString
                ? int.TryParse(nameString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var nameCode)
                    ? ResourceName.FromCode(nameCode)
                    : ResourceName.FromString(nameString)
                : null;

            // List resources
            if (resourceType is null || resourceName is null)
            {
                Console.WriteLine("Resources:");

                foreach (var resource in PortableExecutable.GetResources(imageFilePath))
                {
                    Console.Write("  ");
                    Console.WriteLine(resource);
                }
            }
            // Get specific resource
            else
            {
                var data = PortableExecutable.GetResourceData(
                    imageFilePath,
                    new ResourceIdentifier(resourceType, resourceName)
                );

                var dataString = Encoding.Unicode.GetString(data);

                Console.WriteLine(dataString);
            }
        }
    }
}