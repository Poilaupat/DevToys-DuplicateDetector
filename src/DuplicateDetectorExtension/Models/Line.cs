using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDetectorExtension.Models
{
    /// <summary>
    /// Modelize a line from an input text
    /// </summary>
    internal class Line
    {
        /// <summary>
        /// The line number (one-base index)
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// The line
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// The zero-based index of the start of the line within the original text
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// The offset of the searched part of the line
        /// By default, the value of SearchedOffset is 0 by default when using Line search mode 
        /// </summary>
        public int SearchedOffset { get; private set; }

        /// <summary>
        /// The length of the searched part of the line
        /// By default, the value of SearchedLength is the length of the whole line when using Line search mode 
        /// </summary>
        public int SearchedLength { get; private set; }

        /// <summary>
        /// The searched part of the line
        /// By default, the searched line is the whole line when using Line search mode
        /// </summary>
        public string SearchedValue => Value.Substring(SearchedOffset, SearchedLength);

        public Line(int lineNumber, string line, int lineIndex, int substringOffset, int substringLength)
        {
            LineNumber = lineNumber;
            Value = line;
            Index = lineIndex;
            SearchedOffset = substringOffset;
            SearchedLength = substringLength;
        }
    }
}
