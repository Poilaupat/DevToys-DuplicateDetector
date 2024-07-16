using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DuplicateFinderExtension
{
    internal class LineCollection
    {
        public EDuplicateMode Mode { get; private set; }
        public int SubstringOffset { get; private set; }
        public int SubstringLength { get; private set; }
        public List<Line> Collection { get; } = new List<Line>();

        public LineCollection(EDuplicateMode mode, int substringOffset, int substringLength)
        {
            Mode = mode;
            SubstringOffset = substringOffset;
            SubstringLength = substringLength;
        }

        public void LoadText(string text)
        {
            Collection.Clear();

            var splitter = new Regex("([^\\r\\n]*)(\\r\\n|\\n|$)", RegexOptions.Multiline);
            var matches  = splitter.Matches(text);

            for (int i = 0; i < matches.Count(); i++)
            {
                var value = matches[i].Groups[1].Value;
                var index = matches[i].Groups[1].Index;
                var length = matches[i].Groups[1].Length;
                var substringOffset = Mode == EDuplicateMode.Line ? 0 : SubstringOffset;
                var substringLength = Mode == EDuplicateMode.Line ? 
                    length : 
                    length < SubstringOffset + SubstringLength ? 0 : SubstringLength;

                Collection.Add(new Line(i + 1,
                    value,
                    index,
                    length,
                    substringOffset,
                    substringLength));
            }
        }
    }
}
