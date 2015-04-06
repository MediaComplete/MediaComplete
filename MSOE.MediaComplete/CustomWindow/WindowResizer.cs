using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace MSOE.MediaComplete.CustomWindow
{
    /// <summary>
    /// Class which manages resizing of borderless windows.
    /// Based heavily on Kirupa Chinnathambi's code at http://blog.kirupa.com/?p=256.
    /// 
    /// Obtained from https://bogglex.codeplex.com/SourceControl/changeset/view/9106#241325
    /// </summary>
    public class WindowResizer
    {
        /// <summary>
        /// Defines the cursors that should be used when the mouse is hovering
        /// over a border in each position.
        /// </summary>
        private readonly Dictionary<BorderPosition, Cursor> cursors = new Dictionary<BorderPosition, Cursor>
        {
            { BorderPosition.Left, Cursors.SizeWE },
            { BorderPosition.Right, Cursors.SizeWE },
            { BorderPosition.Top, Cursors.SizeNS },
            { BorderPosition.Bottom, Cursors.SizeNS },
            { BorderPosition.BottomLeft, Cursors.SizeNESW },
            { BorderPosition.TopRight, Cursors.SizeNESW },
            { BorderPosition.BottomRight, Cursors.SizeNWSE },
            { BorderPosition.TopLeft, Cursors.SizeNWSE }
        };

        /// <summary>
        /// The borders for the window.
        /// </summary>
        private readonly WindowBorder[] borders;

        /// <summary>
        /// The handle to the window.
        /// </summary>
        private HwndSource hwndSource;

        /// <summary>
        /// The WPF window.
        /// </summary>
        private readonly Window _window;

        /// <summary>
        /// Creates a new WindowResizer for the specified Window using the
        /// specified border elements.
        /// </summary>
        /// <param name="window">The Window which should be resized.</param>
        /// <param name="borders">The elements which can be used to resize the window.</param>
        public WindowResizer(Window window, params WindowBorder[] borders)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            if (borders == null)
            {
                throw new ArgumentNullException("borders");
            }

            _window = window;
            this.borders = borders;

            foreach (var border in borders)
            {
                border.Element.PreviewMouseLeftButtonDown += Resize;
                border.Element.MouseMove += DisplayResizeCursor;
                border.Element.MouseLeave += ResetCursor;
            }

            window.SourceInitialized += (o, e) => hwndSource = (HwndSource)PresentationSource.FromVisual((Visual)o);
        }

        /// <summary>
        /// Sticks a message on the message queue.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Puts a resize message on the message queue for the specified border position.
        /// </summary>
        /// <param name="direction"></param>
        private void ResizeWindow(BorderPosition direction)
        {
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)direction, IntPtr.Zero);
        }

        /// <summary>
        /// Resets the cursor when the left mouse button is not pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetCursor(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                _window.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Resizes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Resize(object sender, MouseButtonEventArgs e)
        {
            var border = borders.Single(b => b.Element.Equals(sender));
            _window.Cursor = cursors[border.Position];
            ResizeWindow(border.Position);
        }

        /// <summary>
        /// Ensures that the correct cursor is displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayResizeCursor(object sender, MouseEventArgs e)
        {
            var border = borders.Single(b => b.Element.Equals(sender));
            _window.Cursor = cursors[border.Position];
        }

        #region Handle min/max

        // Code adapated from http://stackoverflow.com/questions/1718666
        internal enum WM
        {
            WINDOWPOSCHANGING = 0x0046
        }

        internal enum SWP
        {
            NOMOVE = 0x0002
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
        {
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    {
                        WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
                        if ((pos.flags & (int)SWP.NOMOVE) != 0)
                        {
                            return IntPtr.Zero;
                        }

                        Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                        {
                            return IntPtr.Zero;
                        }

                        bool changedPos = false;

                        // ***********************
                        // Here you check the values inside the pos structure
                        // if you want to override them just change the pos
                        // structure and set changedPos to true
                        // ***********************

                        // this is a simplified version that doesn't work in high-dpi settings
                        // pos.cx and pos.cy are in "device pixels" and MinWidth and MinHeight 
                        // are in "WPF pixels" (WPF pixels are always 1/96 of an inch - if your
                        // system is configured correctly).
                        if (pos.cx < _window.MinWidth) { pos.cx = (int)_window.MinWidth; changedPos = true; }
                        if (pos.cy < _window.MinHeight) { pos.cy = (int)_window.MinHeight; changedPos = true; }


                        // ***********************
                        // end of "logic"
                        // ***********************

                        if (!changedPos)
                        {
                            return IntPtr.Zero;
                        }

                        Marshal.StructureToPtr(pos, lParam, true);
                        handeled = true;
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion
    }
}