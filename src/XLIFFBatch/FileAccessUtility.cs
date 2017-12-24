using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using XLIFFBatch.Models;
using XLIFFBatch.Schema;

namespace XLIFFBatch
{
    public class FileAccessUtility
    {
        public static IEnumerable<Replacement> ReadReplacements(string mapItemDelimiter, string mapItemCommentDelimitter, string mapfilePath)
        {
            var result = new List<Replacement>();
            var delimiters = new string[] { mapItemDelimiter };
            using (var streamReader = new StreamReader(mapfilePath))
            {
                var line = streamReader.ReadLine();
                while (line != null)
                {
                    int commentStart = line.IndexOf(mapItemCommentDelimitter);
                    if (commentStart != -1)
                    {
                        line = line.Substring(0, commentStart);
                    }

                    if (line.Any())
                    {
                        var items = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                        result.Add(new Replacement
                        {
                            SourceString = items[0].Trim(),
                            TargetString = items[1].Trim()
                        });
                    }

                    line = streamReader.ReadLine();
                }
            }

            return result;
        }

        public static IEnumerable<WorkUnit> ReadXliffFiles(IEnumerable<WorkUnit> workUnits)
        {
            return workUnits.Select(workUnit =>
            {
                using (var streamReader = new StreamReader(workUnit.InputFilePath))
                {
                    var serializer = new XmlSerializer(typeof(xliff));
                    workUnit.Xliff = serializer.Deserialize(streamReader) as xliff;
                    return workUnit;
                }
            }).ToArray();
        }

        public static void WriteXllfFile(WorkUnit workUnit)
        {
            using (var streamWriter = new StreamWriter(workUnit.OutputFilePath))
            {
                var serializer = new XmlSerializer(typeof(xliff));
                serializer.Serialize(streamWriter, workUnit.Xliff);
            }
        }
    }
}
