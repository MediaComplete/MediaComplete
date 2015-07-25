using System.Collections.Generic;

namespace MSOE.MediaComplete.Search
{
    public class Results
    {
        public IEnumerable<Entry> Hits { get; set; }
        public int Count { get; set; }
    }
}