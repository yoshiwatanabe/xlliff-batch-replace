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

        public static void WriteXliffFile(WorkUnit workUnit, bool autoCreateOutputDirectory = true)
        {
            var fileInfo = new FileInfo(workUnit.OutputFilePath);
            if (autoCreateOutputDirectory && !Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
                File.Create(workUnit.OutputFilePath).Dispose();
            }

            using (var streamWriter = new StreamWriter(workUnit.OutputFilePath))
            {
                var serializer = new XmlSerializer(typeof(xliff));
                serializer.Serialize(streamWriter, workUnit.Xliff);
            }
        }
    }
}
