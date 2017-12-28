using System;
using System.Linq;
using System.IO;
using XLIFFBatch.Schema;
using Microsoft.Extensions.Configuration;
using XLIFFBatch.Logics;
using XLIFFBatch.Models;
using System.Collections.Generic;

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
            var mapItemCommentDelimitter = Configuration["mapItemCommentDelimitter"];
            var inputDirectory = Configuration["inputDirectory"];
            var outputDirectory = Configuration["outputDirectory"];
            bool replaceWholeWord = bool.Parse(Configuration["options:replaceWholeWord"]);
            bool caseSensitive = bool.Parse(Configuration["options:caseSensitive"]);
            bool processLongerLengthFirst = bool.Parse(Configuration["options:processLongerLengthFirst"]);
            bool autoCreateOutputDirectories = bool.Parse(Configuration["options:autoCreateOutputDirectories"]);
            var inputSearchPattern = Configuration["options:inputSearchPattern"];

            Console.WriteLine($"mapfile: {mapfile}");
            Console.WriteLine($"mapItemDelimiter: {mapItemDelimiter}");
            Console.WriteLine($"mapItemCommentDelimitter: {mapItemCommentDelimitter}");
            Console.WriteLine($"inputDirectory: {inputDirectory}");
            Console.WriteLine($"outputDirectory: {outputDirectory}");
            Console.WriteLine($"processLongerLengthFirst: {processLongerLengthFirst}");
            Console.WriteLine($"autoCreateOutputDirectories: {autoCreateOutputDirectories}");
            Console.WriteLine($"inputSearchPattern: {inputSearchPattern}");

            if (!File.Exists(mapfile) ||
                !Directory.Exists(inputDirectory) ||
                !Directory.Exists(outputDirectory))
            {
                Console.WriteLine("Error: one of the input parameters is invalid. Please check settings.json");
            }

            var options = new Options
            {
                ReplaceWholeWord = replaceWholeWord,
                CaseSensitive = caseSensitive
            };

            var inputRootDirectoryFullPath = inputDirectory;
            if (!Path.IsPathRooted(inputDirectory))
            {
                inputRootDirectoryFullPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\" + inputDirectory);
            }

            var outputRootDirectoryFullPath = outputDirectory;
            if (!Path.IsPathRooted(outputDirectory))
            {
                outputRootDirectoryFullPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\" + outputDirectory);
            }

            var workUnits = CreateWorkUnits(
                inputRootDirectoryFullPath, 
                outputRootDirectoryFullPath,
                inputSearchPattern);

            var replacements = File.ReadLines(Path.Combine(Directory.GetCurrentDirectory(), mapfile)).Select(line =>
            ReplacementParser.Parse(line, mapItemDelimiter, mapItemCommentDelimitter)).Where(_ => _ != null).ToList();

            if (processLongerLengthFirst)
            {
                // This causes search & replace "For Example" before "For", if these two search strings are in the map.
                replacements = replacements.OrderByDescending(_ => _.SourceString.Length).ToList();
            }

            foreach (var workUnit in FileAccessUtility.ReadXliffFiles(workUnits))
            {
                if (SearchAndReplace.Process(replacements, workUnit.Xliff.file[0], options))
                {
                    FileAccessUtility.WriteXliffFile(workUnit, autoCreateOutputDirectories);
                }
            }

            Console.WriteLine("\nDone");
        }

        public static void CreateWorkUnits(string inputDirectory, string outputDirectory, ref List<WorkUnit> workUnits, string searchPattern = "*.*")
        {
            var inputDirectoryInfo = new DirectoryInfo(inputDirectory);            

            workUnits.AddRange(
                inputDirectoryInfo
                    .GetFiles(searchPattern)
                    .Select(_ => _.Name)
                    .Select(name => new WorkUnit
                    {
                        InputFilePath = Path.Combine(inputDirectory, name),
                        OutputFilePath = Path.Combine(outputDirectory, name),
                    })
                );

            foreach (var name in inputDirectoryInfo.GetDirectories().Select(_ => _.Name))
            {
                // Recursive.
                CreateWorkUnits(
                    Path.Combine(inputDirectory, name),
                    Path.Combine(outputDirectory, name),
                    ref workUnits,
                    searchPattern);
            }                
        }

        private static IEnumerable<WorkUnit> CreateWorkUnits(string inputDirectory, string outputDirectory, string searchPattern = "*.*")
        {
            List<WorkUnit> list = new List<WorkUnit>();
            CreateWorkUnits(inputDirectory, outputDirectory, ref list, searchPattern);
            return list;
        }
    }
}
