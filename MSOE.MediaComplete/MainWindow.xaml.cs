using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string Mp3FileFormat = "MP3 Files (*.mp3)|*.mp3";
        private const string FileDialogTitle = "Select Music File(s)";
        private string _homeDir;

        public MainWindow()
        {
            InitializeComponent();
            _homeDir = (string)Properties.Settings.Default["HomeDir"];
            Importer.Instance._homeDir = _homeDir;
			
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
            var fileDialog = new OpenFileDialog
            {
                Filter = Mp3FileFormat,
                InitialDirectory = "C:",
                Title = FileDialogTitle,
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await Task.Run(() => Importer.Instance.ImportFiles(fileDialog.FileNames));
            }

        }

        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedDir = folderDialog.SelectedPath;
                await Task.Run(() => Importer.Instance.ImportDirectory(selectedDir));
                
            }
        }

        public void refreshTreeView(object source, FileSystemEventArgs e)
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(_homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        public void refreshTreeView()
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(_homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        private void initTreeView()
        {
            refreshTreeView();
            
            var watcher = new FileSystemWatcher(_homeDir);
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
        
    }
}
