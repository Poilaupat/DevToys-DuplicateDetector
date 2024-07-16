using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderExtension
{
    internal class Line
    {
        public int LineNumber { get; private set; }
        public string Value { get; private set; }
        public int LineLength { get; private set; }
        public int LineIndex { get; private set; }
        public int SubstringOffset { get; private set; }
        public int SubstringLength { get; private set; }
        public string SearchedValue => Value.Substring(SubstringOffset, SubstringLength);

        public Line(int lineNumber, string line, int lineIndex, int lineLength, int substringOffset, int substringLength)
        {
            LineNumber = lineNumber;
            Value = line;
            LineIndex = lineIndex;
            LineLength = lineLength;
            SubstringOffset = substringOffset;
            SubstringLength = substringLength;
        }
    }
}
