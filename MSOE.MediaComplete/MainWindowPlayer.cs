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
            PlayPauseButton.Style = (Style)FindResource("PlayButton");
            _player = Player.Instance;
        }

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
                    PlaySelectedSong();
                    break;
            }
        }

        private void PlaySelectedSong()
        {
            if (SongTree.SelectedItems == null) return;
            var song = SongTree.SelectedItems.First() as SongTreeViewItem;
            if (song == null) return;
            try
            {
                _player.Play(new FileInfo(song.GetPath()));
            }
            catch (FileLoadException)
            {
                StatusBarHandler.Instance.ChangeStatusBarMessage("CorruptFile-Error", StatusBarHandler.StatusIcon.Error);
                PlayPauseButton.Style = (Style)FindResource("PlayButton");
                return;
            }
            PlayPauseButton.Style = (Style)FindResource("PauseButton");
        }

        private void PauseSong()
        {
            _player.Pause();
            PlayPauseButton.Style = (Style)FindResource("PlayButton");
        }

        private void ResumePausedSong()
        {
            _player.Resume();
            PlayPauseButton.Style = (Style)FindResource("PauseButton");
        }

        private void StopButton_OnClick()
        {
            _player.Stop();
            PlayPauseButton.Style = (Style)FindResource("PlayButton");
        }

        private void SongTree_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedSong();
        }

        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void SkipButton_OnClick(object sender, RoutedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }
    }
}
