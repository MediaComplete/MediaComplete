using System;

namespace MSOE.MediaComplete.Lib.Files
{
    /// <summary>
    /// Represents a song in the error state. The original song (which maybe null) is available via the Source property.
    /// </summary>
    public class ErrorSong : AbstractSong
    {
        /// <summary>
        /// Contains the original song that has a problem. It may be null, if the problem was that the song didn't exist.
        /// </summary>
        public AbstractSong Source { get; private set; }

        /// <summary>
        /// Create an error-state wrapper around a song. 
        /// </summary>
        /// <param name="source">The original song that had the problem. This may be null.</param>
        public ErrorSong(AbstractSong source)
        {
            Source = source;

            _id = Source != null ? Source.Id : Guid.NewGuid().ToString();
        }

        #region AbstractSong overrides

        private readonly string _id;
        public override string Id
        {
            get { return _id; }
        }

        public override bool Equals(object other)
        {
            return other is ErrorSong && ((ErrorSong)other).Id.Equals(_id);
        }

        #endregion
    }
}
