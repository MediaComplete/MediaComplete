using System;
using System.Collections.Generic;
using M3U.NET;
using MSOE.MediaComplete.Lib.Songs;

namespace MSOE.MediaComplete.Lib.Playing
{
    public class PlayQueue
    {
        #region Properties
        public static readonly PlayQueue Inst = new PlayQueue();
        public const string NameString = "Now Playing";
        private readonly List<AbstractSong> _songs = new List<AbstractSong>();
        public IReadOnlyList<AbstractSong> Songs { get { return _songs.AsReadOnly(); } }

        public int Index { get; private set; }
        #endregion

        private PlayQueue()
        {
            Index = -1;
        }

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

        public AbstractSong JumpTo(int i)
        {
            if (i >= _songs.Count || i < 0)
            {
                throw new IndexOutOfRangeException("Cannot jump outside of the playlist.");
            }
            Index = i;
            return _songs[Index];
        }

        public AbstractSong NextSong()
        {
            // TODO MC-38 MC-39 Looping and shuffling logic go here.
            return Index >= _songs.Count - 1 ? null : _songs[++Index];
        }

        public AbstractSong PreviousSong()
        {
            // TODO MC-38 MC-39 Looping and shuffling logic go here.
            return Index <= 0 ? null : _songs[--Index];
        }

        public AbstractSong CurrentSong()
        {
            return Index < 0 ? null : _songs[Index];
        }

        #region List mutators
        public void Append(AbstractSong song)
        {
            _songs.Add(song);
        }

        public void Append(IEnumerable<AbstractSong> songs)
        {
            _songs.AddRange(songs);
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= _songs.Count)
            {
                throw new IndexOutOfRangeException("Index argument must be within the range of the list.");
            }
            if (index <= Index)
            {
                Index--;
            }
            _songs.RemoveAt(index);
        }

        public bool Remove(AbstractSong song)
        {
            var index = _songs.FindIndex(s => s.Equals(song));
            if (index > -1)
            {
                Remove(index);
                return true;
            }
            return false;
        }

        public void Remove(IEnumerable<AbstractSong> songs)
        {
            foreach (var song in songs)
            {
                Remove(song);
            }
        }

        public void Clear()
        {
            _songs.Clear();
            Index = -1;
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || newIndex < 0 || oldIndex >= _songs.Count || newIndex >= _songs.Count)
            {
                throw new IndexOutOfRangeException("Index arguments must be within the range of the list.");
            }
            _songs.Insert(newIndex, _songs[oldIndex]);
            _songs.RemoveAt(oldIndex);
            if (oldIndex == Index)
            {
                Index = newIndex;
            }
        }

        public bool Move(AbstractSong song, int newIndex)
        {
            if (newIndex < 0 || newIndex >= _songs.Count)
            {
                throw new IndexOutOfRangeException("Index arguments must be within the range of the list.");
            }
            var index = _songs.FindIndex(s => s.Equals(song));
            if (index > -1)
            {
                Move(index, newIndex);
                return true;
            }
            return false;
        }
        #endregion

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
    }
}
