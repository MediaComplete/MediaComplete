using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Search.Lucene
{
    /// <summary>
    /// A Lucene-based search index.
    /// 
    /// <see cref="https://lucenenet.apache.org/"/>
    /// </summary>
    internal class LuceneIndex : IIndex
    {
        /// <summary>
        /// Gets or sets the index location. This may not be used in all implementations.
        /// </summary>
        /// <value>
        /// The index location.
        /// </value>
        public string IndexLocation
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Clears the entire index. Useful for when the entire index needs to rebuilt.
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches this index with the specified query.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>A <see cref="Results"/> object</returns>
        public Results Search(Query query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the index entries - either adds or updates them based on the key.
        /// </summary>
        /// <param name="entries">The entries.</param>
        public void UpdateEntries(params Entry[] entries)
        {
            throw new NotImplementedException();
        }
    }
}
