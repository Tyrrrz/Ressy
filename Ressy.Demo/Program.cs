using System;
using System.Linq;
using System.Text;
using Ressy.Identification;

namespace Ressy.Demo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var moduleFilePath = args.ElementAtOrDefault(0) ?? typeof(Program).Assembly.Location;
            using var module = PortableExecutable.FromFile(moduleFilePath);

            var resourceType = args.ElementAtOrDefault(1);
            var resourceName = args.ElementAtOrDefault(2);

            // List resources
            if (string.IsNullOrWhiteSpace(resourceType) || string.IsNullOrWhiteSpace(resourceName))
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
                    new ResourceIdentifier(
                        ResourceType.FromString(resourceType),
                        ResourceName.FromString(resourceName)
                    )
                );

                var dataString = Encoding.Unicode.GetString(data);

                Console.WriteLine(dataString);
            }
        }
    }
}