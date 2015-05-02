using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MSOE.MediaComplete.CustomWindow
{
    /// <summary>
    /// Native Windows API methods and interfaces.
    /// 
    /// Code adapted from http://blog.onedevjob.com/2010/10/19/fixing-full-screen-wpf-windows/
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    internal static class NativeMethods
    {
        #region Constants

        // The WM_GETMINMAXINFO message is sent to a window when the size or
        // position of the window is about to change.
        // An application can use this message to override the window's
        // default maximized size and position, or its default minimum or
        // maximum tracking size.
        internal const int WM_GETMINMAXINFO = 0x0024;

        // Constants used with MonitorFromWindow()
        // Returns NULL.
        internal const int MONITOR_DEFAULTTONULL = 0;

        // Returns a handle to the primary display monitor.
        internal const int MONITOR_DEFAULTTOPRIMARY = 1;

        // Returns a handle to the display monitor that is nearest to the window.
        internal const int MONITOR_DEFAULTTONEAREST = 2;

        #endregion

        #region Structs

        /// <summary>
        /// Native Windows API-compatible POINT struct
        /// </summary>
        [Serializable, StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// The RECT structure defines the coordinates of the upper-left
        /// and lower-right corners of a rectangle.
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/dd162897%28VS.85%29.aspx"/>
        /// <remarks>
        /// By convention, the right and bottom edges of the rectangle
        /// are normally considered exclusive.
        /// In other words, the pixel whose coordinates are ( right, bottom )
        /// lies immediately outside of the the rectangle.
        /// For example, when RECT is passed to the FillRect function, the rectangle
        /// is filled up to, but not including,
        /// the right column and bottom row of pixels. This structure is identical
        /// to the RECTL structure.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            /// <summary>
            /// The x-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            public int Left;

            /// <summary>
            /// The y-coordinate of the upper-left corner of the rectangle.
            /// </summary>
            public int Top;

            /// <summary>
            /// The x-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            public int Right;

            /// <summary>
            /// The y-coordinate of the lower-right corner of the rectangle.
            /// </summary>
            public int Bottom;
        }

        /// <summary>
        /// The MINMAXINFO structure contains information about a window's
        /// maximized size and position and its minimum and maximum tracking size.
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/ms632605%28VS.85%29.aspx"/>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MINMAXINFO
        {
            /// <summary>
            /// Reserved; do not use.
            /// </summary>
            public POINT Reserved;

            /// <summary>
            /// Specifies the maximized width (POINT.x)
            /// and the maximized height (POINT.y) of the window.
            /// For top-level windows, this value
            /// is based on the width of the primary monitor.
            /// </summary>
            public POINT MaxSize;

            /// <summary>
            /// Specifies the position of the left side
            /// of the maximized window (POINT.x)
            /// and the position of the top
            /// of the maximized window (POINT.y).
            /// For top-level windows, this value is based
            /// on the position of the primary monitor.
            /// </summary>
            public POINT MaxPosition;

            /// <summary>
            /// Specifies the minimum tracking width (POINT.x)
            /// and the minimum tracking height (POINT.y) of the window.
            /// This value can be obtained programmatically
            /// from the system metrics SM_CXMINTRACK and SM_CYMINTRACK.
            /// </summary>
            public POINT MinTrackSize;

            /// <summary>
            /// Specifies the maximum tracking width (POINT.x)
            /// and the maximum tracking height (POINT.y) of the window.
            /// This value is based on the size of the virtual screen
            /// and can be obtained programmatically
            /// from the system metrics SM_CXMAXTRACK and SM_CYMAXTRACK.
            /// </summary>
            public POINT MaxTrackSize;
        }

        /// <summary>
        /// The WINDOWINFO structure contains window information.
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/ms632610%28VS.85%29.aspx"/>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWINFO
        {
            /// <summary>
            /// The size of the structure, in bytes.
            /// The caller must set this to sizeof(WINDOWINFO).
            /// </summary>
            public uint Size;

            /// <summary>
            /// Pointer to a RECT structure
            /// that specifies the coordinates of the window.
            /// </summary>
            public RECT Window;

            /// <summary>
            /// Pointer to a RECT structure
            /// that specifies the coordinates of the client area.
            /// </summary>
            public RECT Client;

            /// <summary>
            /// The window styles. For a table of window styles,
            /// <see cref="http://msdn.microsoft.com/en-us/library/ms632680%28VS.85%29.aspx">
            /// CreateWindowEx
            /// </see>.
            /// </summary>
            public uint Style;

            /// <summary>
            /// The extended window styles. For a table of extended window styles,
            /// see CreateWindowEx.
            /// </summary>
            public uint ExStyle;

            /// <summary>
            /// The window status. If this member is WS_ACTIVECAPTION,
            /// the window is active. Otherwise, this member is zero.
            /// </summary>
            public uint WindowStatus;

            /// <summary>
            /// The width of the window border, in pixels.
            /// </summary>
            public uint WindowBordersWidth;

            /// <summary>
            /// The height of the window border, in pixels.
            /// </summary>
            public uint WindowBordersHeight;

            /// <summary>
            /// The window class atom (see
            /// <see cref="http://msdn.microsoft.com/en-us/library/ms633586%28VS.85%29.aspx">
            /// RegisterClass
            /// </see>).
            /// </summary>
            public ushort WindowType;

            /// <summary>
            /// The Windows version of the application that created the window.
            /// </summary>
            public ushort CreatorVersion;
        }

        /// <summary>
        /// The MONITORINFO structure contains information about a display monitor.
        /// The GetMonitorInfo function stores information in a MONITORINFO structure.
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/dd145065%28VS.85%29.aspx"/>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MONITORINFO
        {
            /// <summary>
            /// The size, in bytes, of the structure. Set this member
            /// to sizeof(MONITORINFO) (40) before calling the GetMonitorInfo function.
            /// Doing so lets the function determine
            /// the type of structure you are passing to it.
            /// </summary>
            public int Size;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle,
            /// expressed in virtual-screen coordinates.
            /// Note that if the monitor is not the primary display monitor,
            /// some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT Monitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle
            /// of the display monitor that can be used by applications,
            /// expressed in virtual-screen coordinates.
            /// Windows uses this rectangle to maximize an application on the monitor.
            /// The rest of the area in rcMonitor contains system windows
            /// such as the task bar and side bars.
            /// Note that if the monitor is not the primary display monitor,
            /// some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT WorkArea;

            /// <summary>
            /// The attributes of the display monitor.
            ///
            /// This member can be the following value:
            /// 1 : MONITORINFOF_PRIMARY
            /// </summary>
            public uint Flags;
        }

        #endregion

        #region Imported methods

        /// <summary>
        /// The GetWindowInfo function retrieves information about the specified window.
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/ms633516%28VS.85%29.aspx"/>
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="pwi">The reference to WINDOWINFO structure.</param>
        /// <returns>true on success</returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        /// <summary>
        /// The MonitorFromWindow function retrieves a handle to the display monitor
        /// that has the largest area of intersection with the bounding rectangle
        /// of a specified window.
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/dd145064%28VS.85%29.aspx"/>
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="dwFlags">Determines the function's return value
        /// if the window does not intersect any display monitor.</param>
        /// <returns>
        /// Monitor HMONITOR handle on success or based on dwFlags for failure
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        /// <summary>
        /// The GetMonitorInfo function retrieves information about a display monitor
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/dd144901%28VS.85%29.aspx"/>
        /// </summary>
        /// <param name="hMonitor">A handle to the display monitor of interest.</param>
        /// <param name="lpmi">
        /// A pointer to a MONITORINFO structure that receives information
        /// about the specified display monitor.
        /// </param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        /// <summary>
        /// Retrieve the position of the mouse on the screen
        /// 
        /// See https://msdn.microsoft.com/en-us/library/windows/desktop/ms648390(v=vs.85).aspx
        /// </summary>
        /// <param name="lpPoint">A pointer to a POINT structure that receives the screen coordinates of the cursor.</param>
        /// <returns>Nonzero if successful, zero otherwise</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        #endregion
    }
}