using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace MSOE.MediaComplete.CustomWindow
{
    public class BaseWindow : Window
    {
        public BaseWindow()
        {
            Style = (Style)TryFindResource(typeof (BaseWindow));
        }

        #region Properties

        public static readonly DependencyProperty AllowResizeProperty =
            DependencyProperty.Register("AllowResize", typeof(bool), typeof(BaseWindow),
                new FrameworkPropertyMetadata(true));

        public bool AllowResize
        {
            get { return (bool)GetValue(AllowResizeProperty); }
            set { SetValue(AllowResizeProperty, value); }
        }

        public static readonly DependencyProperty AllowMinimizeProperty =
            DependencyProperty.Register("AllowMinimize", typeof(bool), typeof(BaseWindow),
                new FrameworkPropertyMetadata(true));

        public bool AllowMinimize
        {
            get { return (bool)GetValue(AllowMinimizeProperty); }
            set { SetValue(AllowMinimizeProperty, value); }
        }

        public static readonly DependencyProperty AllowMaximizeProperty =
            DependencyProperty.Register("AllowMaximize", typeof(bool), typeof(BaseWindow),
                new FrameworkPropertyMetadata(true));

        public bool AllowMaximize
        {
            get { return (bool)GetValue(AllowMaximizeProperty); }
            set { SetValue(AllowMaximizeProperty, value); }
        }

        #endregion

        private Button _maxButton;
        private System.Windows.Point? _mousePosition; // Tracks starting mouse position in a drag

        public override void OnApplyTemplate()
        {
            var exitButton = GetTemplateChild("PART_ExitButton") as Button;
            if (exitButton != null)
            {
                exitButton.Click += CloseButton_Click;
            }

            if (AllowMinimize)
            {
                var minButton = GetTemplateChild("PART_MinButton") as Button;
                if (minButton != null)
                {
                    minButton.Click += MinimizeButton_Click;
                }
            }

            if (AllowMaximize)
            {
                var maxButton = GetTemplateChild("PART_MaxButton") as Button;
                if (maxButton != null)
                {
                    _maxButton = maxButton;
                    _maxButton.Click += MaximizeButton_Click;
                }
            }

            var titleBarPanel = GetTemplateChild("PART_TitleBar") as Border;
            if (titleBarPanel != null)
            {
                titleBarPanel.MouseDown += TitleBar_MouseDown;
                if (AllowMaximize)
                {
                    titleBarPanel.MouseMove += TitleBar_MouseMove;
                }
            }

            if (AllowResize)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new WindowResizer(this,
                    new WindowBorder(BorderPosition.Left, GetTemplateChild("PART_LeftBorderHandle") as Rectangle),
                    new WindowBorder(BorderPosition.TopLeft, GetTemplateChild("PART_TopLeftBorderHandle") as Rectangle),
                    new WindowBorder(BorderPosition.Top, GetTemplateChild("PART_TopBorderHandle") as Rectangle),
                    new WindowBorder(BorderPosition.TopRight, GetTemplateChild("PART_TopRightBorderHandle") as Rectangle),
                    new WindowBorder(BorderPosition.Right, GetTemplateChild("PART_RightBorderHandle") as Rectangle),
                    new WindowBorder(BorderPosition.BottomRight,
                        GetTemplateChild("PART_BottomRightBorderHandle") as Rectangle),
                    new WindowBorder(BorderPosition.Bottom, GetTemplateChild("PART_BottomBorderHandle") as Rectangle),
                    new WindowBorder(BorderPosition.BottomLeft,
                        GetTemplateChild("PART_BottomLeftBorderHandle") as Rectangle));
            }

            base.OnApplyTemplate();
        }

        #region Event Handlers

        /// <summary>
        /// TitleBar_MouseDown - Drag if single-click, resize if double-click
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2) // Double-click, just toggle
                {
                    if (AllowMaximize)
                    {
                        AdjustWindowSize();
                    }
                }
                else
                {
                    // Begin drag, record our position
                    _mousePosition = e.GetPosition(this);
                    DragWithSnap();
                }
            }
        }

        /// <summary>
        /// If the user drags on the title bar while maximized, it should restore down and drag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            var newPosition = e.GetPosition(this);
            if (IsDragging(newPosition) && WindowState == WindowState.Maximized)
            {
                // Multi-monitor way to grab the middle of the window.
                double percentHorizontal = newPosition.X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                AdjustWindowSize();

                Point lMousePosition;
                GetCursorPos(out lMousePosition);

                Left = lMousePosition.X - targetHorizontal;
                Top = 0;

                DragWithSnap();
            }
        }

        /// <summary>
        /// CloseButton_Clicked
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// MaximizedButton_Clicked
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        /// <summary>
        /// Minimized Button_Clicked
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            if (_maxButton != null)
                _maxButton.Style = (Style)TryFindResource(WindowState == WindowState.Maximized ? "RestoreDownButtonStyle" : "FullscreenButtonStyle");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper method - returns true if a click-drag is occuring, based on the updated point
        /// </summary>
        /// <param name="newPosition">The location of the new event</param>
        /// <returns>True if we are click-dragging</returns>
        private bool IsDragging(System.Windows.Point newPosition)
        {
            return Mouse.LeftButton == MouseButtonState.Pressed &&  _mousePosition.HasValue &&
                   (Math.Abs(newPosition.X - _mousePosition.Value.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(newPosition.Y - _mousePosition.Value.Y) >= SystemParameters.MinimumVerticalDragDistance);
        }

        /// <summary>
        /// Dragmove and handle snapping logic. Currently only supports fullscreen snapping
        /// </summary>
        private void DragWithSnap()
        {
            DragMove();

            if (AllowMaximize && WindowState == WindowState.Normal)
            {
                Point newPosition;
                GetCursorPos(out newPosition);
                var closeToTop = Math.Abs(SystemParameters.WorkArea.Top - newPosition.Y) < SystemParameters.MinimumVerticalDragDistance;

                if (closeToTop)
                    AdjustWindowSize();
                // TODO MC-270 - support left/right screen snapping as well.

                _mousePosition = null;
            }
        }

        #endregion

        #region Interop mouse position

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out Point lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public readonly int X;
            public readonly int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        #endregion
    }
}
