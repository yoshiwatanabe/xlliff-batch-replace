using System;
using System.Linq;
using System.IO;
using XLIFFBatch.Schema;
using Microsoft.Extensions.Configuration;
using XLIFFBatch.Logics;
using XLIFFBatch.Models;

namespace XLIFFBatch
{   
    class Program
    {
        static void Main(string[] args)
        {
            var Configuration = (new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json")).Build();

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
            
            var workUnits = (new DirectoryInfo(inputDirectory))
                .GetFiles()
                .Select(_ => new Tuple<string, string>(_.FullName, _.Name))
                .Select(t => new WorkUnit
            {
                InputFilePath = t.Item1,
                OutputFilePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), outputDirectory), t.Item2),
            });

            var replacements = FileAccessUtility.ReadReplacements(mapItemDelimiter, Path.Combine(Directory.GetCurrentDirectory(), mapfile));

            foreach (var workUnit in FileAccessUtility.ReadXliffFiles(workUnits))
            {
                if (SearchAndReplace.Process(replacements, workUnit.Xliff.file[0]))
                {
                    FileAccessUtility.WriteXllfFile(workUnit);
                }
            }

            Console.WriteLine("Done");
        }
    }
}
