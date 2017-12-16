using System;
using System.Linq;
using System.IO;
using System.Reflection;
using XLIFFBatch.Schema;
using Microsoft.Extensions.Configuration;
using XLIFFBatch.Models;
using System.Collections.Generic;

namespace XLIFFBatch
{
    
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings.json");

            var Configuration = builder.Build();

            var mapfile = Configuration["mapfile"];
            var mapItemDelimiter = Configuration["mapItemDelimiter"];
            var inputDirectory = Configuration["inputDirectory"];
            var outputDirectory = Configuration["outputDirectory"];

            Console.WriteLine($"mapfile: {mapfile}");
            Console.WriteLine($"mapItemDelimiter: {mapItemDelimiter}");
            Console.WriteLine($"inputDirectory: {inputDirectory}");
            Console.WriteLine($"outputDirectory: {outputDirectory}");

            if (!File.Exists(mapfile) ||
                !Directory.Exists(inputDirectory) ||
                !Directory.Exists(outputDirectory))
            {
                Console.WriteLine("Error: one of the input parameters is invalid. Please check settings.json");
            }

            var replacements = FileAccessUtility.ReadReplacements(mapItemDelimiter, Path.Combine(Directory.GetCurrentDirectory(), mapfile));
            var xliffs = FileAccessUtility.ReadXliffFiles((new DirectoryInfo(inputDirectory)).GetFiles().Select(_ => _.FullName));

            // TODO: scan can replace
        }

        private static void NewMethod(TextReader str)
        {
            System.Xml.Serialization.XmlSerializer xSerializer = new System.Xml.Serialization.XmlSerializer(typeof(xliff));
            var xliff = xSerializer.Deserialize(str) as xliff;
            str.Close();
        }
    }
}
