using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLIFFBatch.Models;

namespace XLIFFBatch.Logics
{
    public class ReplacementParser
    {
        public static Replacement Parse(string line, string delimiter, string mapItemCommentDelimitter)
        {
            var delimiters = new string[] { delimiter };

            int commentStart = line.IndexOf(mapItemCommentDelimitter);
            if (commentStart != -1)
            {
                line = line.Substring(0, commentStart);
            }

            if (line.Any())
            {
                var items = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length == 2)
                {
                    return new Replacement
                    {
                        SourceString = items[0].Trim(),
                        TargetString = items[1].Trim()
                    };
                }
            }

            return null;
        }
    }
}
