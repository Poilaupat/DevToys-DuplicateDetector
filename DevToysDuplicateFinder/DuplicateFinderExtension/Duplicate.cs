using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderExtension
{
    internal class Duplicate
    {
        public string Value { get; private set; }
        public int Count => LineNbrs.Count;
        public List<int> LineNbrs { get; private set; } = new List<int>();

        public Duplicate(string value) 
        {
            Value = value;
        }
    }
}
