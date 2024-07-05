using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderExtension
{
    internal class LineItem
    {
        public EDuplicateMode Mode {  get; private set; }
        public string Line { get; private set; }
        public int LineLength { get; private set; }
        public int LineIndex { get; private set; }
        public int LineNumber { get; private set; }
        public int SubstringOffset { get; private set; }
        public int SubstringLength { get; private set; }
        public string SearchedLinePart 
        {
            get
            {
                if (Mode == EDuplicateMode.OffsetLength)
                {
                    if (Line.Length >= SubstringOffset + SubstringLength)
                    {
                        return Line.Substring(SubstringOffset, SubstringLength);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return Line;
                }
            }
        }

        public LineItem(EDuplicateMode mode, int lineNumber, string line, int lineIndex, int lineLength, int substringOffset, int substringLength)
        {
            Mode = mode;
            LineNumber = lineNumber;
            Line = line;
            LineIndex = lineIndex;
            LineLength = lineLength;
            SubstringOffset = substringOffset;
            SubstringLength = substringLength;
        }
    }
}
