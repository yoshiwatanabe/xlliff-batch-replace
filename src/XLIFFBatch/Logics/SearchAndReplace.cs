using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLIFFBatch.Models;
using XLIFFBatch.Schema;

namespace XLIFFBatch.Logics
{
    public class SearchAndReplace
    {
        public static string[] SkipLineHints = new string[]
        {
            "courses/csintro",
            "<code>"
        };

        public static bool Process(IEnumerable<Replacement> replacements, file file)
        {
            bool replaced = false;

            foreach (transunit unit in file.body)
            {
                var statement = unit.target.Text[0];

                if (SkipLineHints.Any(_ => statement.IndexOf(_) != -1))
                {
                    continue;
                }                
                
                foreach (var replacement in replacements)
                {
                    if (statement.IndexOf(replacement.SourceString) != -1)
                    {
                        statement = statement.Replace(replacement.SourceString, replacement.TargetString);
                        unit.target.state = "translated";
                        replaced = true;
                    }
                }

                if (replaced)
                {
                    unit.target.Text[0] = statement;
                }
            }

            return replaced;
        }
    }
}
