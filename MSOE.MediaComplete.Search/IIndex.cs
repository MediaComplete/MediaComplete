using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Search
{
    /// <summary>
    /// Service providing access to a search index. Index may be searched, or modified
    /// </summary>
    public interface IIndex
    {
        /// <summary>
        /// Gets or sets the index location. This may not be used in all implementations.
        /// </summary>
        /// <value>
        /// The index location.
        /// </value>
        string IndexLocation { get; set; }

        /// <summary>
        /// Searches this index with the specified query.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>A <see cref="Results"/> object</returns>
        Results Search(Query query);

        /// <summary>
        /// Updates the index entries - either adds or updates them based on the key.
        /// </summary>
        /// <param name="entries">The entries.</param>
        void UpdateEntries(params Entry[] entries);

        /// <summary>
        /// Clears the entire index. Useful for when the entire index needs to rebuilt.
        /// </summary>
        void Clear();
    }
}
