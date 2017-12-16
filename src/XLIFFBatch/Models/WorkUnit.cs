using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLIFFBatch.Schema;

namespace XLIFFBatch.Models
{
    public class WorkUnit
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
        public xliff Xliff { get; set; }
    }
}
