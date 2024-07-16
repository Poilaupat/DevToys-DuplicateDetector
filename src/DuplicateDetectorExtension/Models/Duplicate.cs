using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDetectorExtension.Models
{
    /// <summary>
    /// Modelize a duplicate
    /// </summary>
    internal class Duplicate
    {
        /// <summary>
        /// The duplicated value
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// The total number of occurences of the duplicated value
        /// </summary>
        public int Count => LineNbrs.Count;

        /// <summary>
        /// The occurences line numbers
        /// </summary>
        public List<int> LineNbrs { get; private set; } = new List<int>();

        public Duplicate(string value)
        {
            Value = value;
        }
    }
}
