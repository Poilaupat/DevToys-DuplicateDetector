using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDetectorExtension.Models
{
    internal class DuplicateSearchResult
    {
        public LineCollection Lines { get; private set; }
        public List<Duplicate> Duplicates { get; private set; }

        public DuplicateSearchResult(LineCollection lines, List<Duplicate> duplicates)
        {
            Lines = lines;
            Duplicates = duplicates;
        }
    }
}
