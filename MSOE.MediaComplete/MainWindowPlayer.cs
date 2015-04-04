using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Playing;
using MSOE.MediaComplete.Lib.Songs;
using NAudio.Wave;
using TagLib;

namespace MSOE.MediaComplete
{
    partial class MainWindow
    {
        /// <summary>
        /// the player object
        /// </summary>
        private IPlayer _player;

        /// <summary>
        /// initializes the player
        /// </summary>
        private void InitPlayer()
        {
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
            _player = Player.Instance;
            _player.PlaybackEnded += AutomaticStop;
            _player.ChangeVolume(VolumeSlider.Value);
        }

        /// <summary>
        /// A bindable flag for the now playing queue
        /// </summary>
        public ObservableBool NowPlayingDirty { get { return _nowPlayingDirty; } }
        private readonly ObservableBool _nowPlayingDirty = new ObservableBool { Value = true };

        #region Event Handlers
        /// <summary>
        /// Starts, pauses, or resumes the playback appropriately based on the state of the player. 
        /// 
        /// If the player is stopped, and no song is selected, the previously queued music will play.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayPauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            switch (_player.PlaybackState)
            {
                case PlaybackState.Paused:
                    Resume();
                    break;
                case PlaybackState.Playing:
                    Pause();
                    break;
                default:
                    var newSongs = (from LibrarySongItem song in SongList.Items
                                    select new LocalSong(new FileInfo(song.GetPath())));
                    if (newSongs.Any())
                    {
                        NowPlaying.Inst.Clear();
                        NowPlaying.Inst.Add(newSongs);
                        NowPlaying.Inst.JumpTo(SongList.SelectedIndex);
                    }
                    Play();
                    break;
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
            if (SongList.SelectedItems.Count == 0) return;
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
            var list = (from LibrarySongItem song in SongList.SelectedItems select 
                            new LocalSong(new FileInfo(song.GetPath()))).Cast<AbstractSong>().ToList();
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
            //TODO: MC-34 or MC-35
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// Advance the Now Playing queue and player to the next song.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkipButton_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO: MC-34 or MC-35
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// plays the selected song on double click of a song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongTree_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NowPlaying.Inst.Clear();
            AddAllSongsToNowPlaying();
            if (SongList.SelectedItems.Count > 0)
            {
                var songTreeViewItem = SongList.SelectedItems[0] as LibrarySongItem;
                if (songTreeViewItem != null)
                    NowPlaying.Inst.JumpTo(new LocalSong(new FileInfo(songTreeViewItem.GetPath())));
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
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
        }

        /// <summary>
        /// Resumes the song and changes the UI button into a pause button
        /// </summary>
        private void Resume()
        {
            _player.Resume();
            PlayPauseButton.SetResourceReference(StyleProperty, "PauseButton");
            
        }

        /// <summary>
        /// Stops the currently playing song, and clears the Now Playing queue.
        /// </summary>
        private void Stop()
        {
            _player.Stop();
            NowPlaying.Inst.Clear();
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
            if(_visibleList.Equals(PlaylistSongs) && PlaylistList.SelectedIndex==0) 
                PlaylistSongs.Items.Clear();
        }

        /// <summary>
        /// Triggered by the player running out of music, resets the play button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AutomaticStop(object sender, EventArgs eventArgs)
        {
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
            NowPlaying.Inst.Add((from LibrarySongItem song in SongList.Items
                                 select new LocalSong(new FileInfo(song.GetPath()))));
            _nowPlayingDirty.Value = true;
        }

        /// <summary>
        /// Helper method to queue up select songs from the song treeview
        /// </summary>
        private void AddSelectedSongsToNowPlaying()
        {
            NowPlaying.Inst.Add((from LibrarySongItem song in SongList.SelectedItems
                                 select new LocalSong(new FileInfo(song.GetPath()))));
            _nowPlayingDirty.Value = true;
        }
        #endregion
    }
}
