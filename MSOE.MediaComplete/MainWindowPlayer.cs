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
        /// event handler for when the player hits the end of the file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AutomaticStop(object sender, EventArgs eventArgs)
        {
            Stop();
        }

        /// <summary>
        /// starts, pauses, or resumes the playback appropriately based on the state of the player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayPauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            switch (_player.PlaybackState)
            {
                case PlaybackState.Paused:
                    ResumePausedSong();
                    break;
                case PlaybackState.Playing:
                    PauseSong();
                    break;
                default:
                    var song = NowPlaying.Inst.CurrentSong() ??
                               new LocalSong(new FileInfo(((SongTreeViewItem) SongTree.SelectedItems[0]).GetPath()));
                    PlaySelectedSong(song);
                    break;
            }
        }

        /// <summary>
        /// gets the selected song from the song tree and plays it with the Player
        /// </summary>
        private void PlaySelectedSong(AbstractSong song)
        {
            if (SongTree.SelectedItems == null) return;
            if (song == null) return;
            try
            {
                _player.Play(song);
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
        /// pauses the song and changes the UI button into a play button
        /// </summary>
        private void PauseSong()
        {
            _player.Pause();
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
        }

        /// <summary>
        /// resumes the song and changes the UI button into a pause button
        /// </summary>
        private void ResumePausedSong()
        {
            _player.Resume();
            PlayPauseButton.SetResourceReference(StyleProperty, "PauseButton");
        }

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
        /// 
        /// </summary>
        private void Stop()
        {
            _player.Stop();
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
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
            var songTreeViewItem = SongTree.SelectedItems[0] as SongTreeViewItem;
            if (songTreeViewItem != null)
                NowPlaying.Inst.JumpTo(new LocalSong(new FileInfo(songTreeViewItem.GetPath())));
            PlaySelectedSong(NowPlaying.Inst.CurrentSong());
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO: MC-34 or MC-35
            //throw new System.NotImplementedException();
        }

        private void SkipButton_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO: MC-34 or MC-35
            //throw new System.NotImplementedException();
        }

        private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CheckVolumeLevel();
            if(_player!=null)
                _player.ChangeVolume(VolumeSlider.Value);
        }

        private void CheckVolumeLevel()
        {
            if(VolumePercentLabel!=null)
                VolumePercentLabel.FontWeight = VolumeSlider.Value > 100 ? FontWeights.ExtraBold : FontWeights.Normal;
        }

        /// <summary>
        /// plays the selected song on the context menu "Play" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_PlaySongMusic_Click(object sender, RoutedEventArgs e)
        {
            NowPlaying.Inst.Clear();
            NowPlaying.Inst.Add(new LocalSong(new FileInfo(((SongTreeViewItem)SongTree.SelectedItems[0]).GetPath())));
            PlaySelectedSong(NowPlaying.Inst.CurrentSong());
        }

        /// <summary>
        /// Adds the selected song(s) to the now playing queue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddSongToNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            NowPlaying.Inst.Add((from SongTreeViewItem song in SongTree.SelectedItems
                                 select new LocalSong(new FileInfo(song.GetPath())))
                                    .Cast<AbstractSong>().ToList());
            if (Player.Instance.PlaybackState.Equals(PlaybackState.Stopped))
            {
                PlaySelectedSong(NowPlaying.Inst.SongCount() > 1
                    ? NowPlaying.Inst.NextSong()
                    : NowPlaying.Inst.CurrentSong());
            }
            
        }

        /// <summary>
        /// Adds the selected folder(s) of songs to the now playing queue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddFolderToNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            var initialCount = NowPlaying.Inst.SongCount();
            AddAllSongsToNowPlaying();
            if (Player.Instance.PlaybackState.Equals(PlaybackState.Stopped))
            {
                PlaySelectedSong(NowPlaying.Inst.SongCount() > initialCount
                    ? NowPlaying.Inst.NextSong()
                    : NowPlaying.Inst.CurrentSong());
            }
        }

        /// <summary>
        /// Replaces the current now-playing queue with the selected folder(s).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_PlayFolderMusic_Click(object sender, RoutedEventArgs e)
        {
            NowPlaying.Inst.Clear();
            AddAllSongsToNowPlaying();
            PlaySelectedSong(NowPlaying.Inst.CurrentSong());
        }
        
        /// <summary>
        /// Helper method to queue up songs from the song treeview
        /// </summary>
        private void AddAllSongsToNowPlaying()
        {
            NowPlaying.Inst.Add((from SongTreeViewItem song in SongTree.Items
                                 select new LocalSong(new FileInfo(song.GetPath())))
                                    .Cast<AbstractSong>().ToList());
        }
    }
}
