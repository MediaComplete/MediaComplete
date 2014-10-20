using MSOE.MediaComplete.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // TODO - this is temporary code; will be replaced when we can trigger this from context menus on the treeviews
            ((Button)sender).SetValue(Button.ContentProperty, "Loading...");
            string name = await MusicIdentifier.IdentifySong(@"C:\Users\Foxgang\Downloads\03 - Schmerzen.mp3");
            MessageBox.Show(name);
            ((Button)sender).SetValue(Button.ContentProperty, "Done");
        }
    }
}
