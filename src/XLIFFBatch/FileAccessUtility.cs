using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XLIFFBatch.Models;
using XLIFFBatch.Schema;

namespace XLIFFBatch
{
    public class FileAccessUtility
    {

        public static IEnumerable<WorkUnit> ReadXliffFiles(IEnumerable<WorkUnit> workUnits)
        {
            return workUnits.Select(workUnit =>
            {
                using (var streamReader = new StreamReader(workUnit.InputFilePath, Encoding.UTF8))
                {
                    var serializer = new XmlSerializer(typeof(xliff));
                    workUnit.Xliff = serializer.Deserialize(streamReader) as xliff;
                    return workUnit;
                }
            }).ToArray();
        }

        public static void WriteXliffFile(WorkUnit workUnit, bool autoCreateOutputDirectory = true)
        {
            var fileInfo = new FileInfo(workUnit.OutputFilePath);
            if (autoCreateOutputDirectory && !Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                File.Create(workUnit.OutputFilePath).Dispose();
            }

            var ws = new XmlWriterSettings
            {
                NewLineHandling = NewLineHandling.Entitize,
                Indent = true
            };

            var ser = new XmlSerializer(typeof(xliff));
            using (XmlWriter wr = XmlWriter.Create(workUnit.OutputFilePath, ws))
            {
                ser.Serialize(wr, workUnit.Xliff);
            }

            // Quick & dirty replace all for newline char. To keep the original style.
            string xml = File.ReadAllText(workUnit.OutputFilePath);
            xml = xml.Replace("&#xD", "&#13");
            File.WriteAllText(workUnit.OutputFilePath, xml);

            var sourceLines = File.ReadAllLines(workUnit.InputFilePath);
            var lines = File.ReadAllLines(workUnit.OutputFilePath);

            // Some post-fix ups
            if (sourceLines.Length == lines.Length)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    // If it is already approved, don't touch it - swap with the original line.
                    if (line.Contains("approved=\"yes\""))
                    {
                        int pos = (line.IndexOf("identifier"));
                        if (pos != -1)
                        {
                            var check = new string(line.Substring(pos).SkipWhile(c => !Char.IsWhiteSpace(c)).ToArray());
                            if (sourceLines[i].Contains(check))
                            {
                                lines[i] = sourceLines[i]; // Use the original to keep DIFF easier to read.
                            }                            
                        }                        
                    }

                    // If we are not supposed to translate this line, then swap with the original to leave it as it was.
                    if (line.Contains("translate=\"no\""))
                    {
                        int pos = (line.IndexOf("identifier"));
                        if (pos != -1)
                        {
                            var check = new string(line.Substring(pos).SkipWhile(c => !Char.IsWhiteSpace(c)).ToArray());
                            if (sourceLines[i].Contains(check))
                            {
                                lines[i] = sourceLines[i]; // Use the original to keep DIFF easier to read.
                            }
                        }
                    }

                    // Double quates is converted to '"'. We want to preserve the original "&quot;"
                    if (line.Contains("<source>") || line.Contains("<target"))
                    {
                        lines[i] = line.Substring(0, line.IndexOf('>')) + line.Substring(line.IndexOf('>')).Replace("\"", "&quot;");
                    }
                }
            }

            File.WriteAllLines(workUnit.OutputFilePath, lines);

        }
    }
}
