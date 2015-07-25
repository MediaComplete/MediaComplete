using System.Collections.Generic;

namespace MSOE.MediaComplete.Search
{
    /// <summary>
    /// Represents the results of a search
    /// </summary>
    public class Results
    {
        /// <summary>
        /// Gets or sets the search hits.
        /// </summary>
        /// <value>
        /// The hits.
        /// </value>
        public IEnumerable<Entry> Hits { get; set; }

        /// <summary>
        /// Gets or sets the number of hits.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; set; }
    }
}