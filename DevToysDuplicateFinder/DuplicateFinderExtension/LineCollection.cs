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
        public List<LineItem> Lines { get; } = new List<LineItem>();

        public LineCollection(EDuplicateMode mode, int substringOffset, int substringLength)
        {
            Mode = mode;
            SubstringOffset = substringOffset;
            SubstringLength = substringLength;
        }

        public void LoadText(string text)
        {
            Lines.Clear();

            var splitter = new Regex("(.*)(\\r\\n|\\n|$)", RegexOptions.Multiline);
            var matches  = splitter.Matches(text);

            for (int i = 0; i < matches.Count(); i++)
            {
                Lines.Add(new LineItem(Mode,
                    i,
                    matches[i].Groups[1].Value,
                    matches[i].Groups[1].Index,
                    matches[i].Groups[1].Length,
                    SubstringOffset,
                    SubstringLength));
            }
        }
    }
}
