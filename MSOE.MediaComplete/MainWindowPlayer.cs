using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Threading;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib;
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
        /// initializes the player
        /// </summary>
        private void InitPlayer()
        {
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
            _player = Player.Instance;
            _player.PlaybackEnded += AutomaticStop;

            //TrackBar.AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(TrackBar_OnPreviewMouseDown));
            //TrackBar.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(TrackBar_OnPreviewMouseDown));

            //Thumb t = TrackBar.

            TrackBar.ApplyTemplate();

            //TrackBar.dra

            var thumb = (TrackBar.Template.FindName("PART_Track", TrackBar) as Track).Thumb;

            thumb.AddHandler(MouseEnterEvent, new MouseEventHandler(Thumb_MouseEnter));
            TrackBar.AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(Thumb_MouseDown));
            thumb.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Thumb_MouseDown));
            //thumb.AddHandler(MouseUpEvent, new MouseButtonEventHandler(Thumb_MouseUp));
        }

        private void Thumb_MouseDown(object sender, MouseButtonEventArgs args)
        {
            _trackBarUpdateTimer.Elapsed -= CheckTrackBarUpdateTimer;
            _trackBarUpdateTimer.Stop();
            _trackBarUpdateCancellationTokenSource.Cancel();
            _trackBarUpdateCancellationTokenSource = new CancellationTokenSource();
        }

        private void Thumb_MouseUp(object sender, MouseButtonEventArgs args)
        {
            _trackBarUpdateTimer.Elapsed += CheckTrackBarUpdateTimer;
            _trackBarUpdateTimer.Start();
        }

        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            // found here: https://social.msdn.microsoft.com/Forums/vstudio/en-US/5fa7cbc2-c99f-4b71-b46c-f156bdf0a75a/making-the-slider-slide-with-one-click-anywhere-on-the-slider?forum=wpf
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
                (sender as Thumb).RaiseEvent(args);
            }
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
                    PlaySelectedSong();
                    break;
            }
        }

        private const int TimerFrequency = 100;

        private readonly Timer _trackBarUpdateTimer = new Timer(TimerFrequency);
        private CancellationTokenSource _trackBarUpdateCancellationTokenSource = new CancellationTokenSource();
        private void CheckTrackBarUpdateTimer(object obj, ElapsedEventArgs args)
        {
            Dispatcher.Invoke(() => TrackBar.Value = _player.CurrentTime.TotalMilliseconds, DispatcherPriority.DataBind, _trackBarUpdateCancellationTokenSource.Token, TimeSpan.FromMilliseconds(TimerFrequency));
        }

        /// <summary>
        /// gets the selected song from the song tree and plays it with the Player
        /// </summary>
        private void PlaySelectedSong()
        {
            if (SongTree.SelectedItems == null) return;
            var song = SongTree.SelectedItems.First() as SongTreeViewItem;
            if (song == null) return;
            try
            {
                _player.Play(new FileInfo(song.GetPath()));
                TrackBar.Value = 0;
                TrackBar.Minimum = 0;
                TrackBar.Maximum = _player.TotalTime.TotalMilliseconds;
                TrackBar.DataContext = this;
                _trackBarUpdateTimer.Start();
                _trackBarUpdateTimer.Elapsed += CheckTrackBarUpdateTimer;
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
            _trackBarUpdateTimer.Elapsed -= CheckTrackBarUpdateTimer;
            _trackBarUpdateTimer.Stop();
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
        }

        /// <summary>
        /// resumes the song and changes the UI button into a pause button
        /// </summary>
        private void ResumePausedSong()
        {
            _player.Resume();
            _trackBarUpdateTimer.Elapsed += CheckTrackBarUpdateTimer;
            _trackBarUpdateTimer.Start();
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
            _trackBarUpdateTimer.Elapsed -= CheckTrackBarUpdateTimer;
            _trackBarUpdateTimer.Stop();
            TrackBar.Value = 0;
            PlayPauseButton.SetResourceReference(StyleProperty, "PlayButton");
        }

        /// <summary>
        /// plays the selected song on double click of a song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongTree_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedSong();
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
            //_player.Seek();
        }

        /// <summary>
        /// plays the selected song on the context menu "Play" option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_PlayMusic_Click(object sender, RoutedEventArgs e)
        {
            PlaySelectedSong();
        }

        private void TrackBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var diff = Math.Abs(e.OldValue - e.NewValue);
            if (diff > TimerFrequency*2)
            {
                var slider = sender as Slider;
                if (slider != null)
                {
                    var milliseconds = slider.Value;
                    Dispatcher.Invoke(() =>
                    {
                        _player.Seek(TimeSpan.FromMilliseconds(milliseconds));
                    });
                }
            }

        }
    }
}
