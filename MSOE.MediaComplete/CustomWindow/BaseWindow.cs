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

        public override void OnApplyTemplate()
        {
            var dependencyObject = GetTemplateChild("PART_ExitButton");
            if (dependencyObject != null)
            {
                (dependencyObject as Button).Click += CloseButton_Click;
            }

            if (AllowMinimize)
            {
                dependencyObject = GetTemplateChild("PART_MinButton");
                if (dependencyObject != null)
                {
                    (dependencyObject as Button).Click += MinimizeButton_Click;
                }
            }

            if (AllowMaximize)
            {
                dependencyObject = GetTemplateChild("PART_MaxButton");
                if (dependencyObject != null)
                {
                    _maxButton = dependencyObject as Button;
                    _maxButton.Click += MaximizeButton_Click;
                }
            }

            dependencyObject = GetTemplateChild("PART_TitleBar");
            if (dependencyObject != null)
            {
                (dependencyObject as DockPanel).MouseDown += TitleBar_MouseDown;
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

        /// <summary>
        /// TitleBar_MouseDown - Drag if single-click, resize if double-click
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    DragMove();
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
    }
}
