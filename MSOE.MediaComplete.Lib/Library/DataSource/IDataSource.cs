using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Files;

namespace MSOE.MediaComplete.Lib.Library.DataSource
{
    /// <summary>
    /// Interface for any datasource
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// writes a song object to the data source
        /// </summary>
        /// <param name="file"></param>
        void SaveSong(AbstractSong file);
        /// <summary>
        /// Delete a song from the Data source
        /// </summary>
        /// <param name="deletedSong"></param>
        void DeleteSong(AbstractSong deletedSong);
        /// <summary>
        /// Return all song files
        /// </summary>
        /// <returns></returns>
        IEnumerable<AbstractSong> GetAllSongs();
    }
}
