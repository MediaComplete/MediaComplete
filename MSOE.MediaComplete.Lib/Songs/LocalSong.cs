using System;
using System.IO;
using M3U.NET;
using TagLib;
using File = TagLib.File;

namespace MSOE.MediaComplete.Lib.Songs
{
    /// <summary>
    /// Implementation of AbstractSong for local files. This is used for MP3 and WMA files.
    /// </summary>
    public class LocalSong : AbstractSong
    {
        private readonly FileInfo _file;

        /// <summary>
        /// Constructs a LocalSong from a MediaItem
        /// </summary>
        /// <param name="m">The media item</param>
        public LocalSong (MediaItem m) : this(new FileInfo(m.Location))
        {
        }

        /// <summary>
        /// Constructs a LocalSong from a file
        /// </summary>
        /// <param name="file">The file</param>
        public LocalSong(FileInfo file)
        {
            _file = file;
        }
        
        /// <summary>
        /// Converts this LocalSong to a MediaItem so it can be serialized to a playlist.
        /// </summary>
        /// <returns>A new media item</returns>
        public override MediaItem ToMediaItem()
        {
            File tagFile;
            try
            {
                tagFile = File.Create(_file.FullName);
            }
            catch (Exception e)
            {
                if (e is UnsupportedFormatException || e is CorruptFileException)
                {
                    // TODO MC-125 log
                    throw new FileNotFoundException(
                        String.Format("File ({0}) was not found or is not a recognized music file.", _file.FullName), e);
                }

                throw;
            }

            return new MediaItem
            {
                Location = _file.FullName, 
                Inf = tagFile.Tag.FirstAlbumArtist + " - " + tagFile.Tag.Title, 
                Runtime = (int?)tagFile.Properties.Duration.TotalSeconds
            };
        }
    }
}
