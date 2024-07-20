using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DuplicateDetectorExtension.Models
{
    /// <summary>
    /// Modelize a text as a collection of lines
    /// Compatible with \r\n and \n line separators
    /// </summary>
    internal class LineCollection
    {
        /// <summary>
        /// The search mode Line or Offset/Length
        /// </summary>
        public EDuplicateMode Mode { get; private set; }

        /// <summary>
        /// The user parametrized offset value
        /// </summary>
        public int? Offset { get; private set; }

        /// <summary>
        /// The user parametrized length value
        /// </summary>
        public int? Length { get; private set; }

        /// <summary>
        /// The line collection
        /// </summary>
        public List<Line> Collection { get; } = new List<Line>();

        public LineCollection(EDuplicateMode mode, int? substringOffset = null, int? substringLength = null)
        {
            if(mode == EDuplicateMode.OffsetLength && (substringOffset is null || substringLength is null))
                throw new ArgumentNullException($"{nameof(substringOffset)} and {nameof(substringLength)} cannot be null if mode is Offset Length");

            Mode = mode;
            Offset = substringOffset;
            Length = substringLength;
        }

        /// <summary>
        /// Transforms a text in a collection of lines. 
        /// Keep tracks of usefull data like the position of the line within the original text
        /// </summary>
        /// <param name="text">The original text</param>
        public void LoadText(string text)
        {
            Collection.Clear();

            var splitter = new Regex("([^\\r\\n]*)(\\r\\n|\\n|$)", RegexOptions.Multiline);
            var matches = splitter.Matches(text);

            for (int i = 0; i < matches.Count(); i++)
            {
                var value = matches[i].Groups[1].Value;
                var index = matches[i].Groups[1].Index;
                var searchedOffset = Mode == EDuplicateMode.Line ?
                    0 :
                    Offset > value.Length ? 0 : (int)Offset!;
                var searchedLength = Mode == EDuplicateMode.Line ?
                    value.Length :
                    value.Length < Offset + Length ? 0 : (int)Length!;

                Collection.Add(new Line(i + 1,
                    value,
                    index,
                    searchedOffset,
                    searchedLength));
            }
        }
    }
}
