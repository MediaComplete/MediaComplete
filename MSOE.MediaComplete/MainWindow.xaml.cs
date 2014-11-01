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
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using MSOE.MediaComplete.CustomControls;
using Microsoft.Win32;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly string LibraryDirName = "library";

        private string libraryDir;

        public MainWindow()
        {
            InitializeComponent();
            var homeDir = (string)Properties.Settings.Default["HomeDir"];
            libraryDir = homeDir;
            if (!homeDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                libraryDir += Path.DirectorySeparatorChar;
            }
            libraryDir += LibraryDirName + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(libraryDir);
            initTreeView();
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
            if (fileDialog.ShowDialog(this) == true)
            {
                foreach (string file in fileDialog.FileNames)
                {
                    try
                    {
                        System.IO.File.Copy(file.ToString(), libraryDir + System.IO.Path.GetFileName(file));
                        //Console.WriteLine(homeDir + System.IO.Path.GetFileName(file));
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
            WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedDir = folderDialog.SelectedPath;
                string[] files = Directory.GetFiles(selectedDir, "*.mp3",
                                         SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        System.IO.File.Copy(file.ToString(),
                            libraryDir + System.IO.Path.GetFileName(file));

                        //Console.WriteLine(homeDir + System.IO.Path.GetFileName(file));
                    }
                    catch (Exception exception)
                    {
                        System.Console.WriteLine(exception);
                    }
                }
            }
        }

        public void refreshTreeView(object source, FileSystemEventArgs e)
        {
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(libraryDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        public void refreshTreeView()
        {
            // TODO this will cause ugly flickering when we have a ton of files
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(libraryDir);
            var tree = CreateDirectoryItem(rootDirInfo);
            tree.Header = libraryDir;
            LibraryTree.Items.Add(tree);
        }

        private void initTreeView()
        {
            refreshTreeView();

            var watcher = new FileSystemWatcher(libraryDir);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnChanged);

            watcher.EnableRaisingEvents = true;
        }

        private TreeViewItem CreateDirectoryItem(DirectoryInfo dirInfo)
        {
            var dirItem = new FolderTreeViewItem { Header = dirInfo.Name };
            foreach (var dir in dirInfo.GetDirectories())
            {
                dirItem.Items.Add(CreateDirectoryItem(dir));
            }

            foreach (var file in dirInfo.GetFiles())
            {
                dirItem.Items.Add(new SongTreeViewItem
                {
                    Header = file.Name,
                    ContextMenu = LibraryTree.Resources["SongContextMenu"] as System.Windows.Controls.ContextMenu
                });
            }

            return dirItem;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {

            App.Current.Dispatcher.Invoke(new Action(() =>
            {

                var win = App.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                win.refreshTreeView();

            }));

        }

        private async void Toolbar_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            // TODO support multi-select
            var selection = LibraryTree.SelectedItem as TreeViewItem;
            string result = await MusicIdentifier.IdentifySong(selection.FilePath());
            System.Windows.MessageBox.Show(result);
        }

        private async void ContextMenu_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            // TODO support multi-select
            var selection = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TreeViewItem;
            string result;
            // TODO probably don't need to display results. This will be phased out later.
            try
            {
                result = await MusicIdentifier.IdentifySong(selection.FilePath());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                result = null;
            }
            if (result != null)
            {
                MessageBox.Show(result);
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
