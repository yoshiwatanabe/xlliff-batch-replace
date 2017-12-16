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
        public static IEnumerable<Replacement> ReadReplacements(string mapItemDelimiter, string mapfilePath)
        {
            var result = new List<Replacement>();
            var delimiters = new string[] { mapItemDelimiter };
            using (var streamReader = new StreamReader(mapfilePath))
            {
                var line = streamReader.ReadLine();
                while (line != null)
                {
                    var items = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    result.Add(new Replacement
                    {
                        SourceString = items[0].Trim(),
                        TargetString = items[1].Trim()
                    });

                    line = streamReader.ReadLine();
                }
            }

            return result;
        }

        public static IEnumerable<xliff> ReadXliffFiles(IEnumerable<string> xliffFilePaths)
        {
            return xliffFilePaths.Select(_ =>
            {
                using (var streamReader = new StreamReader(_))
                {
                    var xSerializer = new XmlSerializer(typeof(xliff));
                    return xSerializer.Deserialize(streamReader) as xliff;
                }
            }).ToArray();
        }
    }
}
