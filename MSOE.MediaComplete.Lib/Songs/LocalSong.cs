using System;
using System.IO;
using M3U.NET;
using MSOE.MediaComplete.Lib.Files;
using TagLib;
using File = TagLib.File;

namespace MSOE.MediaComplete.Lib.Songs
{
    /// <summary>
    /// Implementation of AbstractSong for local files. This is used for MP3 and WMA files.
    /// </summary>
    public class LocalSong : AbstractSong
    {
        public FileInfo File { get; private set; }

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
            File = file;
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
                tagFile = TagLib.File.Create(File.FullName);
            }
            catch (Exception e)
            {
                if (e is UnsupportedFormatException || e is CorruptFileException)
                {
                    // TODO MC-125 log
                    throw new FileNotFoundException(
                        String.Format("File ({0}) was not found or is not a recognized music file.", File.FullName), e);
                }

                throw;
            }

            return new MediaItem
            {
                Location = File.FullName, 
                Inf = tagFile.Tag.FirstAlbumArtist + " - " + tagFile.Tag.Title, 
                Runtime = (int?)tagFile.Properties.Duration.TotalSeconds
            };
        }

        /// <summary>
        /// Compares two local songs based on whether they refer to the same file.
        /// </summary>
        /// <param name="other">Another object to check</param>
        /// <returns>True if other is a local song referring to the same file, false otherwise.</returns>
        public override bool Equals(object other)
        {
            var otherSong = other as LocalSong;
            return otherSong != null && new FileLocationComparator().Equals(File, otherSong.File);
        }

        public override string ToString()
        {
            return File.Name;
        }

        public override string GetPath()
        {
            return File.FullName; 
        }
    }
}
