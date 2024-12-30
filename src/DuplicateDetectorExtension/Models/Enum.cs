using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateDetectorExtension.Models
{
    /// <summary>
    /// The duplicate search mode
    /// </summary>
    public enum EDuplicateMode
    {
        Line,
        OffsetLength,
    }

    public enum ERemoveDuplicateMode
    {
        KeepFirstOccurence,
        KeepLastOccurence,
        RemoveAll,
    }
}
