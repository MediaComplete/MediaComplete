using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using MSOE.MediaComplete.CustomControls;
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
            var libraryDir = _homeDir;
            if (!_homeDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                libraryDir += Path.DirectorySeparatorChar;
            }

            Directory.CreateDirectory(libraryDir);
            Importer.Instance.HomeDir = libraryDir;
            InitTreeView();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new Settings();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
            if (settingsWindow.DialogResult.GetValueOrDefault(false))
            {
                _homeDir = (string)Properties.Settings.Default["HomeDir"];
                Importer.Instance.HomeDir = _homeDir;
                RefreshTreeView(); 
            }

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
                await Importer.Instance.ImportFiles(fileDialog.FileNames);
            }

            RefreshTreeView();
        }

        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedDir = folderDialog.SelectedPath;
                await Importer.Instance.ImportDirectory(selectedDir);
            }

            RefreshTreeView();
        }

        public void RefreshTreeView()
        {
            //Create Parent node
            var firstNode = new FolderTreeViewItem { Header = "element", ParentItem = null};
            
            SongTree.Items.Clear();

            
            var rootDirInfo = new DirectoryInfo(_homeDir);
            //For each folder in the root Directory
            foreach (var rootChild in rootDirInfo.GetDirectories())
            {
               //add each child to the root folder
               firstNode.Children.Add(PopulateFromFolder(rootChild, SongTree, firstNode));
            }
            foreach (var rootChild in rootDirInfo.GetFiles())
            {
                if (rootChild.Name.EndsWith(".mp3"))
                {
                    SongTree.Items.Add(new SongTreeViewItem { Header = rootChild.Name, ParentItem = firstNode});
                }
            }

            DataContext = firstNode;
        }

        private void InitTreeView()
        {
            RefreshTreeView();

            var watcher = new FileSystemWatcher(_homeDir);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;

            watcher.EnableRaisingEvents = true;
        }

       
        private static FolderTreeViewItem PopulateFromFolder(DirectoryInfo dirInfo, TreeViewEx songTree, FolderTreeViewItem parent)
        {
            var dirItem = new FolderTreeViewItem() { Header = dirInfo.Name, ParentItem = parent};
            foreach (var dir in dirInfo.GetDirectories())
            {
                dirItem.Children.Add(PopulateFromFolder(dir, songTree, dirItem));
            }

            foreach (var file in dirInfo.GetFiles())
            {
                if (file.Name.EndsWith(".mp3"))
                {
                    songTree.Items.Add(new SongTreeViewItem { Header = file.Name, ParentItem = dirItem});
                }
            }
            return dirItem;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {

        }


        private async void Toolbar_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            // TODO support multi-select
            var selection = SongTree.LastSelectedItem as TreeViewItem;
            if (selection is SongTreeViewItem)
            {
                Console.Out.WriteLine("  s"+selection.FilePath());
                string result = await MusicIdentifier.IdentifySong(selection.FilePath());
                System.Windows.Forms.MessageBox.Show(result);
            }
        }

        private async void ContextMenu_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            // TODO support multi-select
            var selection = ((sender as System.Windows.Controls.MenuItem).Parent as System.Windows.Controls.ContextMenu).PlacementTarget as TreeViewItem;
            string result;
            // TODO probably don't need to display results. This will be phased out later.
            try
            {
                result = await MusicIdentifier.IdentifySong(selection.FilePath());
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                result = null;
            }
            if (result != null)
            {
                System.Windows.Forms.MessageBox.Show(result);
            }
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