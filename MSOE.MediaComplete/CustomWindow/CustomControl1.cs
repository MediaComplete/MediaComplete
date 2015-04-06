using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSOE.MediaComplete.CustomWindow
{
    public class CustomControl1 : ContentControl
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }

        private Button _maxButton;

        public override void OnApplyTemplate()
        {
            var dependencyObject = GetTemplateChild("PART_ExitButton");
            if (dependencyObject != null)
            {
                (dependencyObject as Button).Click += CloseButton_Click;
            }

            dependencyObject = GetTemplateChild("PART_MinButton");
            if (dependencyObject != null)
            {
                (dependencyObject as Button).Click += MinimizeButton_Click;
            }

            dependencyObject = GetTemplateChild("PART_MaxButton");
            if (dependencyObject != null)
            {
                _maxButton = dependencyObject as Button;
                _maxButton.Click += MaximizeButton_Click;
            }

            dependencyObject = GetTemplateChild("PART_TitleBar");
            if (dependencyObject != null)
            {
                (dependencyObject as DockPanel).MouseDown += TitleBar_MouseDown;
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
                    Application.Current.MainWindow.DragMove();
                }
        }

        /// <summary>
        /// CloseButton_Clicked
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            var window = Window.GetWindow(this);
            window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            _maxButton.Style = (Style)TryFindResource(window.WindowState == WindowState.Maximized ? "RestoreDownButtonStyle" : "FullscreenButtonStyle");
        }
    }
}
