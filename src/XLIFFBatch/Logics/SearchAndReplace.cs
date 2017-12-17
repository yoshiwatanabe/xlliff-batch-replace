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

        public static bool Process(IEnumerable<Replacement> replacements, file file, Options options)
        {
            bool replaced = false;

            Console.WriteLine("Process file unit started...");

            StringComparison stringComparisonOption = options.CaseSensitive ? 
                StringComparison.InvariantCulture : 
                StringComparison.InvariantCultureIgnoreCase;

            foreach (transunit unit in file.body)
            {
                Console.Write("#");
           
                var statement = unit.target.Text[0];

                if (SkipLineHints.Any(_ => statement.IndexOf(_) != -1))
                {
                    continue;
                }                
                
                foreach (var replacement in replacements)
                {
                    int pos = statement.IndexOf(replacement.SourceString, stringComparisonOption);
                    while (pos != -1 && 
                        (options.ReplaceWholeWord && TestWholeWord(replacement.SourceString, pos, statement))
                        )
                    {
                        statement = $"{statement.Substring(0, pos)}{replacement.TargetString}{statement.Substring(pos + replacement.SourceString.Length)}";                        
                        replaced = true;
                        pos = statement.IndexOf(replacement.SourceString, pos+replacement.TargetString.Length, stringComparisonOption);
                    }
                }

                if (replaced)
                {
                    unit.target.state = "translated";
                    unit.target.Text[0] = statement;
                }
            }

            return replaced;
        }

        private static bool TestWholeWord(string sourceString, int pos, string statement)
        {
            return (pos != 0
                && char.IsWhiteSpace(statement[pos - 1])
                && (
                    pos + sourceString.Length == statement.Length
                    || 
                    (pos + sourceString.Length < statement.Length 
                        && char.IsWhiteSpace(statement[pos + sourceString.Length]))));
        }
    }
}
