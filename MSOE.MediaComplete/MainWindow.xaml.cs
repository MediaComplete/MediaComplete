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
    public partial class MainWindow
    {
        private const string Mp3FileFormat = "MP3 Files (*.mp3)|*.mp3";
        private const string FileDialogTitle = "Select Music File(s)";
        private readonly string _homeDir;
        private readonly Importer _importer;

        public MainWindow()
        {
            InitializeComponent();
            _homeDir = (string)Properties.Settings.Default["HomeDir"];
            if (_homeDir.EndsWith("\\"))
            {
                _homeDir += "library\\";
            }
            else
            {
                _homeDir += "\\library\\";
            }

            Directory.CreateDirectory(_homeDir);
            _importer = new Importer(_homeDir);
            InitTreeView();
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
                await Task.Run(() => _importer.ImportFiles(fileDialog.FileNames));
            }

        }

        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedDir = folderDialog.SelectedPath;
                await Task.Run(() => _importer.ImportDirectory(selectedDir));

            }
        }

        public void RefreshTreeView()
        {
            FolderTree.Items.Clear();
            SongTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(_homeDir);
            foreach (var rootChild in rootDirInfo.GetDirectories())
            {
                FolderTree.Items.Add(PopulateFromFolder(rootChild, SongTree));
            }
            foreach (var rootChild in rootDirInfo.GetFiles())
            {
                SongTree.Items.Add(new SongTreeViewItem {Header = rootChild.Name});
            }
            
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

       
        private static FolderTreeViewItem PopulateFromFolder(DirectoryInfo dirInfo, TreeViewEx songTree)
        {
            var dirItem = new FolderTreeViewItem() { Header = dirInfo.Name };
            foreach (var dir in dirInfo.GetDirectories())
            {
                dirItem.Items.Add(PopulateFromFolder(dir, songTree));
            }

            foreach (var file in dirInfo.GetFiles())
            {
                songTree.Items.Add(new SongTreeViewItem { Header = file.Name });
            }
            
            return dirItem;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
            }));
        }


        private async void Toolbar_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            // TODO support multi-select
            //var selection = LibraryTree.SelectedItem as TreeViewItem;
            //if (selection == null || !(selection is SongTreeViewItem))
            //{
            //    return;
            //}
            //else
            //{
            //    string result = await MusicIdentifier.IdentifySong(selection.FilePath());
            //    System.Windows.Forms.MessageBox.Show(result);
            //}
        }

        private async void ContextMenu_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            //// TODO support multi-select
            //var selection = ((sender as System.Windows.Controls.MenuItem).Parent as System.Windows.Controls.ContextMenu).PlacementTarget as TreeViewItem;
            //string result;
            //// TODO probably don't need to display results. This will be phased out later.
            //try
            //{
            //    result = await MusicIdentifier.IdentifySong(selection.FilePath());
            //}
            //catch (Exception ex)
            //{
            //    System.Windows.Forms.MessageBox.Show(ex.Message);
            //    result = null;
            //}
            //if (result != null)
            //{
            //    System.Windows.Forms.MessageBox.Show(result);
            //}
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