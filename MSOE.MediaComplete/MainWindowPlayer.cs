using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib;
using NAudio.Wave;

namespace MSOE.MediaComplete
{
    partial class MainWindow
    {
        private Player _player;

        private void InitPlayer()
        {
            _player = Player.Instance;
        }

        private void PlayPauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            switch (_player.PlaybackState())
            {
                case PlaybackState.Paused:
                    ResumePausedSong();
                    break;
                case PlaybackState.Playing:
                    PauseSong();
                    break;
                default:
                    PlaySelectedSong();
                    break;
            }
        }

        private void PlaySelectedSong()
        {
            if (SongTree.SelectedItems == null) return;
            var song = SongTree.SelectedItems.First() as SongTreeViewItem;
            if (song == null) return;
            _player.Play(new FileInfo(song.GetPath()));
            PlayPauseButton.Content = "Pause";//TODO: replace with proper line to change button to pause icon
        }

        private void PauseSong()
        {
            _player.Pause();
            PlayPauseButton.Content = "Play";//TODO: replace with proper line to change button to play icon
        }

        private void ResumePausedSong()
        {
            _player.Resume();
            PlayPauseButton.Content = "Pause";//TODO: replace with proper line to change button to pause icon
        }

        private void StopButton_OnClick()
        {
            _player.Stop();
            PlayPauseButton.Content = "Play";//TODO: replace with proper line to change button to play icon
        }

        private void SongTree_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedSong();
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void SkipButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
