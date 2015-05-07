using System;
using System.Collections.Generic;
using System.Linq;
using M3U.NET;
using MSOE.MediaComplete.Lib.Playlists;
using MSOE.MediaComplete.Lib.Files;

namespace MSOE.MediaComplete.Lib.Playing
{
    /// <summary>
    /// Represents the Now Playing queue of the application. The Player draws on this for songs, 
    /// and it can be displayed and edited on the frontend as a playlist
    /// </summary>
    public class NowPlaying
    {
        #region Properties
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static readonly NowPlaying Inst = new NowPlaying();

        /// <summary>
        /// The human-readable name for the "playlist" that's pumped through to the UI.
        /// </summary>
        public const string NameString = "Now Playing";

        /// <summary>
        /// Privately controlled list of songs in the queue.
        /// </summary>
        private readonly List<AbstractSong> _songs = new List<AbstractSong>();

        /// <summary>
        /// Index used to point to the currently active song. 
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Readonly view of the play queue, as a playlist.
        /// </summary>
        public Playlist Playlist
        {
            get
            {
                var pl = new Playlist(new FakeM3U());
                pl.Songs.AddRange(_songs);
                return pl;
            }
        }
        #endregion

        /// <summary>
        /// Private constructor. This class is a singleton.
        /// </summary>
        private NowPlaying()
        {
            Index = -1;
        }

        #region Playing queue stuff

        /// <summary>
        /// Advance the queue to the given song.
        /// </summary>
        /// <param name="song">The song to skip ahead to.</param>
        /// <returns>True if the song exists in the queue, false otherwise.</returns>
        public bool JumpTo(AbstractSong song)
        {
            if (song == null)
                throw new ArgumentNullException("song", "Song parameter must not be null");

            var newIndex = _songs.FindIndex(s => s.Equals(song));
            if (newIndex > -1)
            {
                Index = newIndex;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Advances the queue to the song at the given index.
        /// </summary>
        /// <param name="index">The index to skip to.</param>
        /// <returns>The song at the index</returns>
        /// <exception cref="IndexOutOfRangeException">If the provided index is outside the range of the queue.</exception>
        public AbstractSong JumpTo(int index)
        {
            if (index >= _songs.Count || index < 0)
            {
                throw new IndexOutOfRangeException("Cannot jump outside of the playlist.");
            }
            Index = index;
            return _songs[Index];
        }

        /// <summary>
        /// Returns true if the queue has another song in the playlist, respective to looping and shuffling rules.
        /// </summary>
        /// <returns>True if calling NextSong() will return a song</returns>
        public bool HasNextSong()
        {
            // TODO MC-38 MC-39 Looping and shuffling logic go here.
            return Index < _songs.Count-1 && Index >= 0;
        }

        /// <summary>
        /// Advances to the next song in the queue, respecting any shuffle/loop settings that are enabled.
        /// </summary>
        /// <returns>The chosen song</returns>
        public AbstractSong NextSong()
        {
            // TODO MC-38 MC-39 Looping and shuffling logic go here.
            return Index >= _songs.Count - 1 ? null : _songs[++Index];
        }

        /// <summary>
        /// Retreats to the previous song in the queue, respecting any shuffle/loop settings that are enabled.
        /// </summary>
        /// <returns>The chosen song</returns>
        public AbstractSong PreviousSong()
        {
            // TODO MC-38 MC-39 Looping and shuffling logic go here.
            return Index <= 0 ? null : _songs[--Index];
        }

        public void InsertRange(int insertIndex, IEnumerable<AbstractSong> songs)
        {
            if(songs==null) throw new ArgumentNullException();
            if (insertIndex > _songs.Count || insertIndex < 0) throw new IndexOutOfRangeException();
            if (_songs.Count == 0) Index = 0;
            _songs.InsertRange(insertIndex, songs);
        }
        /// <summary>
        /// Returns the currently selected song.
        /// </summary>
        /// <returns>The song referred to by Index.</returns>
        public AbstractSong CurrentSong()
        {
            return Index < 0 ? null : _songs[Index];
        }
        #endregion

        #region List mutators
        /// <summary>
        /// Adds the given song to the end of the playing queue.
        /// </summary>
        /// <param name="song">The new song</param>
        /// <exception cref="ArgumentNullException">If song is null</exception>
        public void Add(AbstractSong song)
        {
            if (song == null)
                throw new ArgumentNullException("song", "Song should not be null");
            if (Index == -1) Index = 0;
            _songs.Add(song);
        }

        /// <summary>
        /// Adds a range of songs to the end of the playing queue.
        /// </summary>
        /// <param name="songs">The new songs</param>
        /// <exception cref="ArgumentNullException">If songs is null</exception>
        public void Add(IEnumerable<AbstractSong> songs)
        {
            if (songs == null)
                throw new ArgumentNullException("songs", "Songs must not be null");
            if (Index == -1 && songs.Any()) Index = 0;
            _songs.AddRange(songs);
        }

        /// <summary>
        /// Removes the song at the given index.
        /// </summary>
        /// <param name="index">The target index to remove from</param>
        /// <exception cref="IndexOutOfRangeException">If the index is too large or small for the queue.</exception>
        public void Remove(int index)
        {
            if (index < 0 || index >= _songs.Count)
                throw new IndexOutOfRangeException("Index argument must be within the range of the list.");
            
            if (index <= Index)
            {
                Index--;
                if (Index < 0 && _songs.Count > 1)
                {
                    Index = 0;
                }
            }
            _songs.RemoveAt(index);
        }

        /// <summary>
        /// Removes the first instance of the specified song from the queue. Remove(int index) is 
        /// preferred, since it can more flexibly remove songs that appear multiple times in the 
        /// list.
        /// </summary>
        /// <param name="song">The song to search for and remove</param>
        /// <returns>True if the song was found and removed, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If song is null</exception>
        public bool Remove(AbstractSong song)
        {
            if (song == null)
                throw new ArgumentNullException("song", "Song should not be null");

            var index = _songs.FindIndex(s => s.Equals(song));
            if (index > -1)
            {
                Remove(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove the specified songs from the queue. Any songs that do not exist in the queue are ignored.
        /// </summary>
        /// <param name="songs">The songs to remove</param>
        /// <exception cref="ArgumentNullException">If songs is null</exception>
        public void Remove(IEnumerable<AbstractSong> songs)
        {
            if (songs == null)
                throw new ArgumentNullException("songs", "Songs should not be null");

            foreach (var song in songs)
            {
                Remove(song);
            }
        }

        /// <summary>
        /// Remove all songs from the now playing queue.
        /// </summary>
        public void Clear()
        {
            _songs.Clear();
            Index = -1;
        }

        /// <summary>
        /// Relocate the song at oldIndex to newIndex.
        /// </summary>
        /// <param name="oldIndex">The old location of a song</param>
        /// <param name="newIndex">The new target location</param>
        /// <exception cref="IndexOutOfRangeException">If newIndex or oldIndex are out of bounds of the queue</exception>
        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || newIndex < 0 || oldIndex >= _songs.Count || newIndex >= _songs.Count)
                throw new IndexOutOfRangeException("Index arguments must be within the range of the list.");

            // Shift our pointer as well
            if (oldIndex == Index)
            {
                Index = newIndex;
            }

            // Increment if moving forward -- need to compensate since we're removing our old location.
            if (oldIndex < newIndex)
                newIndex++;

            _songs.Insert(newIndex, _songs[oldIndex]);
            _songs.RemoveAt(oldIndex);
        }

        /// <summary>
        /// Move the first occurance of the given song to the target index.
        /// </summary>
        /// <param name="song">The song to move.</param>
        /// <param name="newIndex">The new index.</param>
        /// <returns>True if the song was found and moved, false otherwise.</returns>
        /// <exception cref="IndexOutOfRangeException">If newIndex is beyond the range of the queue</exception>
        public bool Move(AbstractSong song, int newIndex)
        {
            if (song == null)
                throw new ArgumentNullException("song", "Song must not be null!");

            var index = _songs.FindIndex(s => s.Equals(song));
            if (index > -1)
            {
                Move(index, newIndex);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the total number of songs currently in the playlist
        /// </summary>
        /// <returns>An integer count of the songs.</returns>
        public int SongCount()
        {
            return _songs.Count;
        }

        #endregion

        #region FakeM3U
        /// <summary>
        /// No-op M3U file class. Used to back a "playlist" for the now-playing queue, 
        /// but it obviously won't allow saving, renaming , etc.
        /// </summary>
        private class FakeM3U : IM3UFile
        {
            public void Delete()
            {
            }

            public void Save(bool extendedFormat = true)
            {
            }

            public List<MediaItem> Files
            {
                get { return new List<MediaItem>(); }
            }

            public string Name
            {
                get { return NameString; }
                set { }
            }
        }
        #endregion
    }
}
