using System.Collections.Generic;

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
    }
}
