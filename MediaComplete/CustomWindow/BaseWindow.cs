using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SysInterop = System.Windows.Interop;
using System.Windows.Shapes;

namespace MediaComplete.CustomWindow
{
    public class BaseWindow : Window
    {
        #region Properties and Fields

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

        private Button _maxButton;
        private Point? _mousePosition; // Tracks starting mouse position in a drag

        #endregion

        #region Init

        public BaseWindow()
        {
            Style = (Style)TryFindResource(typeof(BaseWindow));
            SourceInitialized += SetupInteropHooks;
        }

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

        private void SetupInteropHooks(object sender, EventArgs e)
        {
            IntPtr handle = (new SysInterop.WindowInteropHelper(this)).Handle;
            var hwndSource = SysInterop.HwndSource.FromHwnd(handle);
            if (hwndSource != null) hwndSource.AddHook(MaximizedSizeFixWindowProc);
        }

        #endregion

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

                NativeMethods.POINT lMousePosition;
                NativeMethods.GetCursorPos(out lMousePosition);

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
            if (Owner != null)
                Owner.WindowState = WindowState.Minimized;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            if (_maxButton != null)
                _maxButton.Style = (Style)TryFindResource(WindowState == WindowState.Maximized ? "RestoreDownButtonStyle" : "FullscreenButtonStyle");
        }

        /// <summary>
        /// Helper method - returns true if a click-drag is occuring, based on the updated point
        /// </summary>
        /// <param name="newPosition">The location of the new event</param>
        /// <returns>True if we are click-dragging</returns>
        private bool IsDragging(Point newPosition)
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
                NativeMethods.POINT newPosition;
                NativeMethods.GetCursorPos(out newPosition);
                var closeToTop = Math.Abs(SystemParameters.WorkArea.Top - newPosition.Y) < SystemParameters.MinimumVerticalDragDistance;

                if (closeToTop)
                    AdjustWindowSize();
                // TODO MC-49 - support left/right screen snapping as well.

                _mousePosition = null;
            }
        }

        #endregion

        #region Interop maximize without covering the taskbar

        // Following code is adapted from http://blog.onedevjob.com/2010/10/19/fixing-full-screen-wpf-windows/

        /// <summary>
        /// Window procedure callback.
        /// Hooked to a WPF maximized window works around a WPF bug:
        /// https://connect.microsoft.com/VisualStudio/feedback/details/363288/maximised-wpf-window-not-covering-full-screen?wa=wsignin1.0#tabs
        /// possibly also:
        /// https://connect.microsoft.com/VisualStudio/feedback/details/540394/maximized-window-does-not-cover-working-area-after-screen-setup-change?wa=wsignin1.0
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="msg">The window message.</param>
        /// <param name="wParam">The wParam (word parameter).</param>
        /// <param name="lParam">The lParam (long parameter).</param>
        /// <param name="handled">
        /// if set to <c>true</c> - the message is handled
        /// and should not be processed by other callbacks.
        /// </param>
        /// <returns></returns>
        internal IntPtr MaximizedSizeFixWindowProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            switch (msg)
            {
                case NativeMethods.WM_GETMINMAXINFO:
                    // Handle the message and mark it as handled,
                    // so other callbacks do not touch it
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        /// <summary>
        /// Creates and populates the MINMAXINFO structure for a maximized window.
        /// Puts the structure into memory address given by lParam.
        /// Only used to process a WM_GETMINMAXINFO message.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="lParam">The lParam.</param>
        internal void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            // Get the MINMAXINFO structure from memory location given by lParam
            NativeMethods.MINMAXINFO mmi =
                (NativeMethods.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MINMAXINFO));

            // Get the monitor that overlaps the window or the nearest
            IntPtr monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                // Get monitor information
                NativeMethods.MONITORINFO monitorInfo = new NativeMethods.MONITORINFO
                {
                    Size = Marshal.SizeOf(typeof(NativeMethods.MONITORINFO))
                };
                NativeMethods.GetMonitorInfo(monitor, ref monitorInfo);

                // Get window information
                NativeMethods.WINDOWINFO windowInfo = new NativeMethods.WINDOWINFO
                {
                    Size = (UInt32)(Marshal.SizeOf(typeof(NativeMethods.WINDOWINFO)))
                };
                NativeMethods.GetWindowInfo(hwnd, ref windowInfo);

                // Set the dimensions of the window in maximized state
                NativeMethods.RECT rcWorkArea = monitorInfo.WorkArea;
                NativeMethods.RECT rcMonitorArea = monitorInfo.Monitor;
                mmi.MaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                mmi.MaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                mmi.MaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                mmi.MaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);

                mmi.MinTrackSize.X = (int)MinWidth;
                mmi.MinTrackSize.Y = (int)MinHeight;
            }

            // Copy the structure to memory location specified by lParam.
            // This concludes processing of WM_GETMINMAXINFO.
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        #endregion
    }
}
