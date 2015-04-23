using System.Collections.Generic;
using MSOE.MediaComplete.Lib.Metadata;

namespace MSOE.MediaComplete.Lib.Sorting
{
    /// <summary>
    /// Contains the settings for a sort operation
    /// </summary>
    public class SortSettings
    {
        /// <summary>
        /// The order by which metadata attributes should be considered for sorting.
        /// Example: Artist, Year, Album
        /// </summary>
        public List<MetaAttribute> SortOrder { get; set; }

        /// <summary>
        /// The specific files to sort
        /// </summary>
        public IEnumerable<SongPath> Files { get; set; }
    }
}