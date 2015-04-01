using System.Windows;
using MSOE.MediaComplete.Border;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for WindowWrapper.xaml
    /// </summary>
    public partial class WindowWrapper : Window
    {
        public WindowWrapper()
        {
            InitializeComponent();
            new WindowResizer(this,
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
