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
            homeDir = (string)Properties.Settings.Default["HomeDir"];
            if (homeDir.EndsWith("\\"))
            {
                homeDir += "library\\";
            }
            else
            {
                homeDir += "\\library\\";
            }
			
            Directory.CreateDirectory(homeDir);
            initTreeView();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }

        private async void AddFile_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "MP3 Files (*.mp3)|*.mp3";
            fileDialog.InitialDirectory = "C:";
            fileDialog.Title = "Select Music File(s)";
            fileDialog.Multiselect = true;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string filename in fileDialog.FileNames)
                {
                    using (FileStream SourceStream = File.Open(filename, FileMode.Open))
                    {
                        using (FileStream DestinationStream = File.Create(homeDir + System.IO.Path.GetFileName(filename)))
                        {
                            await SourceStream.CopyToAsync(DestinationStream);
                        }
                    }
                }
            }

        }

        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                String selectedDir = folderDialog.SelectedPath;
                String[] files = Directory.GetFiles(selectedDir, "*.mp3",
                                         SearchOption.AllDirectories);
                foreach (String file in files)
                {
                    using (FileStream SourceStream = File.Open(file, FileMode.Open))
                    {
                        using (FileStream DestinationStream = File.Create(homeDir + System.IO.Path.GetFileName(file)))
                        {
                            await SourceStream.CopyToAsync(DestinationStream);
                        }
                    }
                }
            }
        }

        public void refreshTreeView(object source, FileSystemEventArgs e)
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        public void refreshTreeView()
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        private void initTreeView()
        {
            refreshTreeView();
            
            var watcher = new FileSystemWatcher(homeDir);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnChanged);

            watcher.EnableRaisingEvents = true;
        }

        private static TreeViewItem CreateDirectoryItem(DirectoryInfo dirInfo)
        {
            var dirItem = new TreeViewItem { Header = dirInfo.Name };
            foreach (var dir in dirInfo.GetDirectories())
            {
                dirItem.Items.Add(CreateDirectoryItem(dir));
            }

            foreach (var file in dirInfo.GetFiles())
            {
                dirItem.Items.Add(new TreeViewItem { Header = file.Name });
            }

            return dirItem;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {

            App.Current.Dispatcher.Invoke(new Action(() => {

                var win = App.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                win.refreshTreeView();
            
            }));
            
        }

        //private static bool CtrlPressed()
        //{
        //    return System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl) || System.Windows.Input.Keyboard.IsKeyDown(Key.RightCtrl);
        //}

        //private void LibraryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    Console.WriteLine("Sender: " + sender);

        //    TreeViewItem selectedItem = (TreeViewItem) e.NewValue;
        //}
    }
}
