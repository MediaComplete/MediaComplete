using System.Collections.Generic;

namespace MSOE.MediaComplete.Search
{
    /// <summary>
    /// Represents an entry in the search index
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Gets or sets the unique key. This identifies duplicates.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the entries's search-able fields
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public Dictionary<string, string> Fields { get; set; }
    }
}