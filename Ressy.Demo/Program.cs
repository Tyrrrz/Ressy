using System;
using System.Linq;
using System.Text;

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

                foreach (var resource in module.GetResources())
                {
                    Console.Write("  ");
                    Console.WriteLine(resource);
                }
            }
            // Get specific resource
            else
            {
                var data = module.GetResource(
                    ResourceType.FromString(resourceType),
                    ResourceName.FromString(resourceName)
                ).GetData();

                var dataString = Encoding.UTF8.GetString(data);

                Console.WriteLine(dataString);
            }
        }
    }
}