using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Logging;
using MSOE.MediaComplete.Lib.Playing;
using NAudio.Wave;
using TagLib;
using Timer = System.Timers.Timer;

namespace MSOE.MediaComplete
{
    partial class MainWindow
    {
        /// <summary>
        /// the player object
        /// </summary>
        private IPlayer _player;

        /// <summary>
        /// frequency in milliseconds at which to update the trackbar position based on the current time of the song
        /// </summary>
        private const int TimerFrequency = 100;

        /// <summary>
        /// Timer to fire the update of the trackbar position
        /// </summary>
        private readonly Timer _trackBarUpdateTimer = new Timer(TimerFrequency);

        /// <summary>
        /// token source used to cancel dispatched trackbar position update events
        /// </summary>
        private CancellationTokenSource _trackBarUpdateCancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// initializes the player
        /// </summary>
        private void InitPlayer()
        {
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
            _player = Player.Instance;
            _player.PlaybackEnded += AutomaticStop;
            Player.Instance.PlaylistFinishedEvent += PlaylistEnded;
            NowPlaying.Inst.PlaylistEnded += PlaylistEnded;
            _player.ChangeVolume(VolumeSlider.Value);
            Player.Instance.SongFinishedEvent += UpdateColorEvent;
            Player.Instance.SongFinishedEvent += ResetTrackBar;
            TrackBar.ApplyTemplate();

            var track = TrackBar.Template.FindName("PART_Track", TrackBar) as Track;
            if (track != null)
            {
                var thumb = track.Thumb;

                thumb.AddHandler(MouseEnterEvent, new MouseEventHandler(Thumb_MouseEnter));
                TrackBar.AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(Thumb_MouseDown));
                thumb.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Thumb_MouseDown));
            }
            else
            {
                throw new ApplicationException("Trackbar instance doesn't exist");
            }
        }

        /// <summary>
        /// A bindable flag for the now playing queue
        /// </summary>
        public ObservableBool NowPlayingDirty { get { return _nowPlayingDirty; } }
        private readonly ObservableBool _nowPlayingDirty = new ObservableBool { Value = true };

        #region Event Handlers
        /// <summary>
        /// tell the player to seek if the difference in the old and new times is much larger than expected from playing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrackBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var diff = Math.Abs(e.OldValue - e.NewValue);
            var currentTime = TimeSpan.FromMilliseconds(e.NewValue);
            var totalTime = TimeSpan.FromMilliseconds(TrackBar.Maximum);

            var formatString = totalTime.TotalHours >= 1 ? "{0:D2}:{1:D2}:{2:D2}" : "{1:D2}:{2:D2}";

            CurrentTimeLabel.Content = string.Format(formatString, (int)currentTime.TotalHours, currentTime.Minutes, currentTime.Seconds);

            var timeRemaining = totalTime.Subtract(currentTime);

            RemainingTimeLabel.Content = string.Format("-" + formatString, (int)timeRemaining.TotalHours, timeRemaining.Minutes, timeRemaining.Seconds);
            if (diff > TimerFrequency * 2)
            {
                Dispatcher.Invoke(() =>
                {
                    _player.Seek(currentTime);
                });
            }
        }

        /// <summary>
        /// stops listening to the trackbar update event timer, cancels all pending trackbar updates and reinstantiates the cancellation token for future use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Thumb_MouseDown(object sender, MouseButtonEventArgs args)
        {
            StopListeningToUpdateTrackbar();
        }



        /// <summary>
        /// starts listening to the tarckbar update timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Thumb_MouseUp(object sender, MouseButtonEventArgs args)
        {
            StartListeningToUpdateTrackbar();
        }

        /// <summary>
        /// capture mouse events if the thumb moves under the cursor while the left mouse button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            // inital idea for this function found here: https://social.msdn.microsoft.com/Forums/vstudio/en-US/5fa7cbc2-c99f-4b71-b46c-f156bdf0a75a/making-the-slider-slide-with-one-click-anywhere-on-the-slider?forum=wpf
            if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
            {
                // the left button is pressed on mouse enter
                // but the mouse isn't captured, so the thumb
                // must have been moved under the mouse in response
                // to a click on the track.
                // Generate a MouseLeftButtonDown event.
                var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
                Thumb_MouseDown(sender, args);
                args.RoutedEvent = MouseDownEvent;
                var thumb = sender as Thumb;
                if (thumb != null) thumb.RaiseEvent(args);
            }
        }
		
        /// <summary>
        /// Starts, pauses, or resumes the playback appropriately based on the state of the player. 
        /// 
        /// If the player is stopped, and no song is selected, the previously queued music will play.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayPauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.LogInformation("Play button clicked");
            switch (_player.PlaybackState)
            {
                case PlaybackState.Paused:
                    Resume();
                    break;
                case PlaybackState.Playing:
                    Pause();
                    break;
                default:
                    var newSongs = AllSongs();
                if (!newSongs.Any()) return;
                NowPlaying.Inst.Clear();
                NowPlaying.Inst.Add(newSongs.Select(s => s.Data));
                NowPlaying.Inst.JumpTo(0);
                Play();
                break;
            }
        }

        /// <summary>
        /// Update the highlighted song in the Now Playing queue when a song ends.
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        private void UpdateColorEvent(int oldIndex, int newIndex)
        {
            var nowPlayingSongs = (ObservableCollection<SongListItem>) PlaylistSongs.Source;

            if (oldIndex == -1 && newIndex == -1)
            {
                var song = nowPlayingSongs[NowPlaying.Inst.Index];
                song.IsPlaying = false;
            }
            else if (NowPlayingList.IsSelected)
            {
                var oldSong = nowPlayingSongs[oldIndex];
                var newSong = nowPlayingSongs[newIndex];
                oldSong.IsPlaying = false;
                newSong.IsPlaying = true;
            }
        }


        /// <summary>
        /// dispatches an action to update the trackbar value with the songs current position
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private void CheckTrackBarUpdateTimer(object obj, ElapsedEventArgs args)
        {
            try
            {
                Dispatcher.Invoke(() => TrackBar.Value = _player.CurrentTime.TotalMilliseconds,
                    DispatcherPriority.DataBind, _trackBarUpdateCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                //swallow and continue
                //if the task was cancelled, then I don't have to worry about doing it anymore
            }
        }

        #region Context Menu Playing
        /// <summary>
        /// Plays all the songs in the view, from context menu "Play All" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_PlaySongMusic_Click(object sender, RoutedEventArgs e)
        {
            NowPlaying.Inst.Clear();
            AddAllSongsToNowPlaying();
            Play();
        }

        /// <summary>
        /// Plays the selected songs in the view, from context menu "Play" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_PlaySelectedSongs_Click(object sender, RoutedEventArgs e)
        {
            if (!SelectedSongs().Any()) return;
            NowPlaying.Inst.Clear();
            AddSelectedSongsToNowPlaying();
            Play();
        }

        /// <summary>
        /// Adds the currently selected songs to the Now Playing queue, then moves them up 
        /// behind the currently playing song.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_PlaySongNext_Click(object sender, RoutedEventArgs e)
        {
            var count = NowPlaying.Inst.SongCount();
            var list = SelectedSongs().Select(s => s.Data);
            NowPlaying.Inst.InsertRange(NowPlaying.Inst.Index + 1, list);
            _nowPlayingDirty.Value = true;
            if (_player.PlaybackState == PlaybackState.Stopped)
            {
                if (count.Equals(NowPlaying.Inst.Index + 1))
                    NowPlaying.Inst.NextSong();
                Play();
            }
        }

        /// <summary>
        /// Adds the selected song(s) to the now playing queue, from context menu "Add to Now Playing" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddSongToNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            AddSelectedSongsToNowPlaying();
            if (Player.Instance.PlaybackState.Equals(PlaybackState.Stopped))
            {
                if (NowPlaying.Inst.SongCount() > 1)
                {
                    NowPlaying.Inst.NextSong();
                }
                Play();
            }
        }

        /// <summary>
        /// Adds the selected folder(s) of songs to the now playing queue, from context menu "Add to Now Playing" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddFolderToNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            var initialCount = NowPlaying.Inst.SongCount();
            AddAllSongsToNowPlaying();
            if (Player.Instance.PlaybackState.Equals(PlaybackState.Stopped))
            {
                if (initialCount > 0)
                {
                    NowPlaying.Inst.NextSong();
                }
                Play();
            }
        }

        /// <summary>
        /// Replaces the current now-playing queue with the selected folder(s), from context menu "Play" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_PlayFolderMusic_Click(object sender, RoutedEventArgs e)
        {
            NowPlaying.Inst.Clear();
            AddAllSongsToNowPlaying();
            Play();
        }

        #endregion

        /// <summary>
        /// stops the song and changes the UI play/pause button to show the play icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// Jump back to the beginning of the current song, or to the previous song if 
        /// we're already near the beginning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_player.CurrentTime > TimeSpan.FromSeconds(2))
            {
                _player.Seek(TimeSpan.FromSeconds(0));//better way to do this?
            }
            else
            {
                var queue = NowPlaying.Inst;
                var old = queue.Index;
                queue.PreviousSong();
                var current = queue.Index;
                UpdateColorEvent(old, current);
                Play();
            }
        }

        /// <summary>
        /// Advance the Now Playing queue and player to the next song.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkipButton_OnClick(object sender, RoutedEventArgs e)
        {
            var queue = NowPlaying.Inst;
            var old = queue.Index;
            queue.NextSong();
            var current = queue.Index;
            UpdateColorEvent(old, current);
            if (queue.Index != 0)
            {
                Play();
            }
        }

        /// <summary>
        /// plays the selected song on double click of a song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NowPlaying.Inst.Clear();
            AddAllSongsToNowPlaying();
            var selectedItems = SelectedSongs();
            if (selectedItems.Any())
            {
                var songTreeViewItem = selectedItems.FirstOrDefault();
                if (songTreeViewItem != null)
                    NowPlaying.Inst.JumpTo(songTreeViewItem.Data);
                Play();
            }
        }

        private void FolderTree_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FolderTree.SelectedItems.Count > 0)
            {

                NowPlaying.Inst.Clear();
                AddAllSongsToNowPlaying();
                Play();
            }
        }

        /// <summary>
        /// Play songs on double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaylistSongs_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var playlistSongs = (ObservableCollection<SongListItem>)PlaylistSongs.Source;
            if (playlistSongs.Count == 0) return;

            var targetSong = SelectedSongs().FirstOrDefault();

            if (targetSong != null)
            {
                if (NowPlayingList.IsSelected) // Jump in current now playing
                {
                    if (targetSong.IsPlaying) return;
                    targetSong.IsPlaying = true;
                    playlistSongs[NowPlaying.Inst.Index].IsPlaying = false;
                    NowPlaying.Inst.JumpTo(SelectedSongIndex());
                }
                else
                {
                    NowPlaying.Inst.Clear();
                    AddAllSongsToNowPlaying();
                    var selectedItems = SelectedSongs();
                    if (selectedItems.Any())
                    {
                        var songTreeViewItem = selectedItems.FirstOrDefault();
                        if (songTreeViewItem != null)
                            NowPlaying.Inst.JumpTo(songTreeViewItem.Data);
                        Play();//TODO is this necessary?
                    }
                }

                Play();
            }
        }

        /// <summary>
        /// Update the volume.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CheckVolumeLevel();
            if (_player != null)
                _player.ChangeVolume(VolumeSlider.Value);
        }

        /// <summary>
        /// Toggles between looping modes - loop entire, loop one, no loop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoopButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(LoopButton))
            {
                LoopButtonPressed.Visibility = Visibility.Visible;
                LoopButtonOne.Visibility = Visibility.Hidden;
                LoopButton.Visibility = Visibility.Hidden;
            }
            else if (sender.Equals(LoopButtonPressed))
            {
                LoopButtonPressed.Visibility = Visibility.Hidden;
                LoopButtonOne.Visibility = Visibility.Visible;
                LoopButton.Visibility = Visibility.Hidden;
            }
            else if (sender.Equals(LoopButtonOne))
            {
                LoopButtonPressed.Visibility = Visibility.Hidden;
                LoopButtonOne.Visibility = Visibility.Hidden;
                LoopButton.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// Triggers the player, handles any resulting exceptions
        /// </summary>
        private void Play()
        {
            try
            {
                _player.Play();
                ResetTrackBar();
                StatusBarHandler.Instance.ChangeStatusBarMessage(null, StatusBarHandler.StatusIcon.None);
            }
            catch (CorruptFileException)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("CorruptFile-Error", StatusBarHandler.StatusIcon.Error);
                PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
                return;
            }
            catch (FileNotFoundException)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("FileNotFound-Error", StatusBarHandler.StatusIcon.Error);
                PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
                return;
            }
            PlayPauseButton.SetResourceReference(StyleProperty, "PauseButton");
        }

        /// <summary>
        /// Pauses the song and changes the UI button into a play button
        /// </summary>
        private void Pause()
        {
            _player.Pause();
            StopListeningToUpdateTrackbar();
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
        }

        /// <summary>
        /// Resumes the song and changes the UI button into a pause button
        /// </summary>
        private void Resume()
        {
            _player.Resume();
            StartListeningToUpdateTrackbar();
            PlayPauseButton.SetResourceReference(StyleProperty, "PauseButton");
            
        }

        /// <summary>
        /// Stops the currently playing song, and clears the Now Playing queue.
        /// </summary>
        private void Stop()
        {
            _player.Stop();
            StopListeningToUpdateTrackbar();
            TrackBar.Maximum = 1;
            TrackBar.Value = 0;
            TrackBar.IsEnabled = false;
            NowPlaying.Inst.Clear();
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
            ((ObservableCollection<SongListItem>)PlaylistSongs.Source).Clear();
        }

        /// <summary>
        /// Triggered by the player running out of music, resets the play button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AutomaticStop(object sender, EventArgs eventArgs)
        {
            TrackBar.Maximum = 0;
            TrackBar.Value = 0;
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
        }

        /// <summary>
        /// Bolds the volume text if the volume is above the 100% threshold
        /// </summary>
        private void CheckVolumeLevel()
        {
            if (VolumePercentLabel != null)
                VolumePercentLabel.FontWeight = VolumeSlider.Value > 100 ? FontWeights.ExtraBold : FontWeights.Normal;
        }

        /// <summary>
        /// Helper method to queue up songs from the song treeview
        /// </summary>
        private void AddAllSongsToNowPlaying()
        {
            NowPlaying.Inst.Add(AllSongs().Select(s => s.Data));
            _nowPlayingDirty.Value = true;
        }

        /// <summary>
        /// Helper method to queue up select songs from the song treeview
        /// </summary>
        private void AddSelectedSongsToNowPlaying()
        {
            NowPlaying.Inst.Add(SelectedSongs().Select(s => s.Data));
            _nowPlayingDirty.Value = true;
        }

        /// <summary>
        /// helper method to unsubscribe, stop and cancel the trackbar update timer event
        /// </summary>
        private void StopListeningToUpdateTrackbar()
        {
            _trackBarUpdateTimer.Elapsed -= CheckTrackBarUpdateTimer;
            _trackBarUpdateTimer.Stop();
            _trackBarUpdateCancellationTokenSource.Cancel();
            _trackBarUpdateCancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// helper method to subscribe and start the trackbar update timer event
        /// </summary>
        private void StartListeningToUpdateTrackbar()
        {
            _trackBarUpdateTimer.Elapsed += CheckTrackBarUpdateTimer;
            _trackBarUpdateTimer.Start();
        }

        /// <summary>
        /// helper to reset the track bar
        /// </summary>
        /// <param name="oldpath">not used, required to subscribe to SongFinishedEvent</param>
        /// <param name="newpath">not used, required to subscribe to SongFinishedEvent</param>
        private void ResetTrackBar(int oldpath, int newpath)
        {
            ResetTrackBar();
        }

        /// <summary>
        /// resets the track bar to its initial state at the beginning of playing a song
        /// </summary>
        private void ResetTrackBar()
        {
            StopListeningToUpdateTrackbar();
            TrackBar.ValueChanged -= TrackBar_OnValueChanged;
            TrackBar.IsEnabled = true;
            TrackBar.Value = 0;
            TrackBar.Minimum = 0;
            TrackBar.Maximum = _player.TotalTime.TotalMilliseconds;
            TrackBar.DataContext = this;
            TrackBar.ValueChanged += TrackBar_OnValueChanged;
            StartListeningToUpdateTrackbar();
        }
        #endregion


        private void PlaylistEnded()
        {
            _player.Stop();
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
            PlaylistSongList.SelectedIndex = 0;
        }
    }
}
