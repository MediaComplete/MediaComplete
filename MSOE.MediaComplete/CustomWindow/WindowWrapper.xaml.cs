using System.Windows;

namespace MSOE.MediaComplete.CustomWindow
{
    /// <summary>
    /// Interaction logic for WindowWrapper.xaml
    /// </summary>
    public partial class WindowWrapper
    {
        public WindowWrapper()
        {
            InitializeComponent();
            Loaded += SetupResize;
        }

        private void SetupResize(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new WindowResizer(Window.GetWindow(this),
                new WindowBorder(BorderPosition.TopLeft, TopLeft),
                new WindowBorder(BorderPosition.Top, Top),
                new WindowBorder(BorderPosition.TopRight, TopRight),
                new WindowBorder(BorderPosition.Right, Right),
                new WindowBorder(BorderPosition.BottomRight, BottomRight),
                new WindowBorder(BorderPosition.Bottom, Bottom),
                new WindowBorder(BorderPosition.BottomLeft, BottomLeft),
                new WindowBorder(BorderPosition.Left, Left));
        }
    }
}
