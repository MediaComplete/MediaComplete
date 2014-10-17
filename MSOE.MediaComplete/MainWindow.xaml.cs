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
using System.Windows.Forms;
using System.IO;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String homeDir;
        public MainWindow()
        {
            InitializeComponent();
            homeDir =  (string)Properties.Settings.Default["HomeDir"] + "library\\";
            Console.WriteLine(homeDir);
            Directory.CreateDirectory(homeDir);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "MP3 Files (*.mp3)|*.mp3";
            fileDialog.InitialDirectory = "C:";
            fileDialog.Title = "Select Music File(s)";
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String file in fileDialog.FileNames)
                {
                    try
                    {
                        System.IO.File.Copy(file.ToString(),  homeDir + System.IO.Path.GetFileName(file));
                        Console.WriteLine(homeDir + System.IO.Path.GetFileName(file));
                    }
                    catch (Exception exception)
                    {
                        System.Console.WriteLine(exception);
                    }

                }
            }

        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                String selectedDir = folderDialog.SelectedPath;
                String[] files = Directory.GetFiles(selectedDir, "*.mp3",
                                         SearchOption.AllDirectories);
                foreach (String file in files)
                {
                    try
                    {
                        System.IO.File.Copy(file.ToString(),
                            homeDir +  System.IO.Path.GetFileName(file));

                        Console.WriteLine(homeDir + System.IO.Path.GetFileName(file));
                    }
                    catch (Exception exception)
                    {
                        System.Console.WriteLine(exception);
                    }
                }
            }
        }
    }
}
