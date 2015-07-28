using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Library;
using MSOE.MediaComplete.Lib.Library.FileSystem;
using MSOE.MediaComplete.Lib.Sorting;
using MSOE.MediaComplete.Lib.Logging;

namespace MSOE.MediaComplete.Lib.Metadata
{
    /// <summary>
    /// Provides functions for identifying a song
    /// </summary>
    public class Identifier : Background.Task
    {
        #region Properties and constructors

        private const int Freq = 8000;
        private const int SampleSeconds = 10;

        /// <summary>
        /// Controls how music data is read in for identification
        /// </summary>
        private readonly IAudioReader _audioReader;

        /// <summary>
        /// Controls how music data is read in for identification
        /// </summary>
        private readonly IAudioIdentifier _audioIdentifier;

        /// <summary>
        /// Controls where additional metadata is retrieved from after identifying a song.
        /// </summary>
        private IMetadataRetriever _metadataRetriever;

        /// <summary>
        /// Controls how files are accessed and edited
        /// </summary>
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// The collection of songs to identify
        /// </summary>
        public IEnumerable<LocalSong> Files { get; private set; }

        /// <summary>
        /// Use the specified services for identification.
        /// </summary>
        /// <param name="files">The files to identify</param>
        /// <param name="reader">Audio reader for parsing in audio data</param>
        /// <param name="identifier">Audio identifier for fingerprinting songs</param>
        /// <param name="metadata">Metadata retriever for finding additional metadata details.</param>
        /// <param name="library">Controls how files are accessed</param>
        public Identifier(IEnumerable<LocalSong> files, IAudioReader reader, IAudioIdentifier identifier, IMetadataRetriever metadata, IFileSystem fileSystem)
        {
            _audioReader = reader;
            _metadataRetriever = metadata;
            _audioIdentifier = identifier;
            _fileSystem = fileSystem;

            if (files == null)
                throw new ArgumentNullException("files", "Files must not be null");
            Files = files;
        }

        #endregion

        /// <summary>
        /// Identify a song; restoring its metadata based on the audio data
        /// </summary>
        /// <param name="file">The target song</param>
        /// <returns></returns>
        private async Task IdentifySongAsync(LocalSong file)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Started",
                StatusBarHandler.StatusIcon.Working);

            if (!_fileSystem.FileExists(file.SongPath))
            {
                // TODO (MC-125) Logging
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException",
                    StatusBarHandler.StatusIcon.Error);
                return;
            }

            // Read in audio data
            var audioData = await _audioReader.ReadBytesAsync(file, Freq, SampleSeconds);
            if (audioData == null)
            {
                // TODO (MC-125) Logging
                StatusBarHandler.Instance.ChangeStatusBarMessage("MusicIdentification-Error-NoException",
                    StatusBarHandler.StatusIcon.Error);
                return;
            }

            await _audioIdentifier.IdentifyAsync(audioData, file);
            

            if (file.Title == null) // No match. Tell the user they'll have to do it themselves.
            {
                // TODO (MC-125) Logging
                StatusBarHandler.Instance.ChangeStatusBarMessage("{0}: {1}", "MusicIdentification-Warning-NoMatch",
                    StatusBarHandler.StatusIcon.Warning, file.SongPath);
                return;
            }

             // Found a match; populate the file
            if (_metadataRetriever == null)
                _metadataRetriever = await SpotifyMetadataRetriever.GetInstanceAsync();
            await _metadataRetriever.GetMetadataAsync(file);

            // Save whatever we found
            _fileSystem.SaveFile(file);
        }

        #region Task overrides

        /// <summary>
        /// Performs the sort, calculating the necessary actions first, if necessary.
        /// </summary>
        /// <param name="i">The task identifier</param>
        /// <returns>An awaitable task</returns>
        public override void Do(int i)
        {
            Id = i;
            Message = "MusicIdentification-Started";
            Icon = StatusBarHandler.StatusIcon.Working;

            var counter = 0;
            var count = Files.Count();
            var max = (count > 100 ? count/100 : 1);
            var total = 0;
            var stop = false;
            var files = Files.ToList();
            for (var idx = 0; idx < files.Count && !stop; idx++)
            {
                try
                {
                    IdentifySongAsync(files[idx]).Wait();
                }
                catch (AggregateException e) {
                    e.Handle(x =>
                    {
                        if (x is IdentificationException) // API overuse, back off
                        {
                            Logger.LogException("Automatic music identification error", x);
                            Message = "MusicIdentification-Warning-RateLimit";
                            Icon = StatusBarHandler.StatusIcon.Warning;
                            // TODO MC-76 Any other ID tasks in the queue should be canceled somehow
                            TriggerUpdate(this);
                            stop = true;
                        }
                        else
                        {
                            Logger.LogException("Automatic music identification error", x);
                            Error = x;
                            Message = "MusicIdentification-Error";
                            Icon = StatusBarHandler.StatusIcon.Error;
                            TriggerUpdate(this);
                        }
                        return true;
                    });
                }

                total++;
                if (counter++ >= max)
                {
                    counter = 0;
                    PercentComplete = ((double) total)/count;
                    TriggerUpdate(this);
                }
            }

            if (Error == null)
            {
                Message = "MusicIdentification-Success";
                Icon = StatusBarHandler.StatusIcon.Success;
                TriggerUpdate(this);
            }
            TriggerDone(this);
        }

        /// <summary>
        /// Contains any subclass types that cannot appear before this task in the execution queue.
        /// Used by <see cref="MSOE.MediaComplete.Lib.Background.TaskAdder.ResolveConflicts" /> to re-order the queue after adding this task.
        /// </summary>
        public override IReadOnlyCollection<Type> InvalidBeforeTypes
        {
            get { return new List<Type> { typeof(Importer) }.AsReadOnly(); }
        }

        /// <summary>
        /// Contains any subclass types that cannot appear after this task in the execution queue.
        /// Used by <see cref="MSOE.MediaComplete.Lib.Background.TaskAdder.ResolveConflicts" /> to re-order the queue after adding this task.
        /// </summary>
        public override IReadOnlyCollection<Type> InvalidAfterTypes
        {
            get { return new List<Type> { typeof(Sorter) }.AsReadOnly(); }
        }

        /// <summary>
        /// Contains any subclass types that cannot appear in the same parallel block in the execution queue.
        /// Used by <see cref="MSOE.MediaComplete.Lib.Background.TaskAdder.ResolveConflicts" /> to re-order the queue after adding this task.
        /// </summary>
        public override IReadOnlyCollection<Type> InvalidDuringTypes
        {
            get { return new List<Type>().AsReadOnly(); }
        }

        /// <summary>
        /// Remove duplicate songs from the identification list, but don't remove any tasks
        /// </summary>
        /// <param name="t">The other task to consider</param>
        /// <returns>false</returns>
        public override bool RemoveOther(Background.Task t)
        {
            var idTask = t as Identifier;
            if (idTask != null)
            {
                Files = Files.Except(idTask.Files); // Remove duplicates
            }
            return false;
        }

        #endregion
    }
}