using System.Collections.Generic;

namespace MSOE.MediaComplete.Search
{
    /// <summary>
    /// Represents a search query
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Contains key-value pairs of field names and values to search for.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public Dictionary<string, string> FieldQueries { get; set; }
    }
}