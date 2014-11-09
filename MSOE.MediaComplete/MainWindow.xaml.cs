using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MSOE.MediaComplete.Lib;
using WinForms = System.Windows.Forms;
using System.Windows;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib.Sorting;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MSOE.MediaComplete
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string _homeDir;
        private readonly Importer _importer;

        public MainWindow()
        {
            InitializeComponent();

            var homeDir = Properties.Settings.Default["HomeDir"] as string ??
                          Path.GetPathRoot(Environment.SystemDirectory);
            if (!homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                homeDir += Path.DirectorySeparatorChar;
            }
            homeDir += Lib.Constants.LibraryDirName + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(homeDir);
            _homeDir = homeDir;
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
            var fileDialog = new WinForms.OpenFileDialog
            {
                Filter =
                    Resources["Dialog-AddFile-FileFilter"] + "" + Lib.Constants.FileDialogFilterStringSeparator +
                    Lib.Constants.MusicFilePattern,
                InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory),
                Title = Resources["Dialog-AddFile-Title"].ToString(),
                Multiselect = true
            };

            if (fileDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                await Task.Run(() => _importer.ImportFiles(fileDialog.FileNames));
            }
        }

        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedDir = folderDialog.SelectedPath;
                await Task.Run(() => _importer.ImportDirectory(selectedDir));
                
            }
        }

        public void RefreshTreeView()
        {
            // TODO this will cause ugly flickering when we have a ton of files
            LibraryTree.Items.Clear();

            var rootDirInfo = new DirectoryInfo(_homeDir);

            LibraryTree.Items.Add(CreateDirectoryItem(rootDirInfo));
        }

        private void InitTreeView()
        {
            RefreshTreeView();

            var watcher = new FileSystemWatcher(_homeDir)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            };
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;

            watcher.EnableRaisingEvents = true;
        }

        private TreeViewItem CreateDirectoryItem(DirectoryInfo dirInfo)
        {
            var dirItem = new FolderTreeViewItem {Header = dirInfo.Name};
            foreach (var dir in dirInfo.GetDirectories())
            {
                dirItem.Items.Add(CreateDirectoryItem(dir));
            }

            foreach (var file in dirInfo.GetFiles())
            {
                dirItem.Items.Add(new SongTreeViewItem
                {
                    Header = file.Name,
                    ContextMenu = LibraryTree.Resources["SongContextMenu"] as ContextMenu
                });
            }

            return dirItem;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var win = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (win != null)
                {
                    win.RefreshTreeView();
                }
            });
        }

        private async void Toolbar_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            // TODO support multi-select
            var selection = LibraryTree.SelectedItems;
            foreach (var node in selection)
            {
                if (node is SongTreeViewItem)
                {
                    string result = await MusicIdentifier.IdentifySong((node as SongTreeViewItem).FilePath());
                    MessageBox.Show(result);
                }   
            }
        }

        private async void ContextMenu_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            // TODO support multi-select
            var menuItem = sender as MenuItem;
            if (menuItem == null)
                return;
            var contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu == null)
                return;
            var selection = contextMenu.PlacementTarget as TreeViewItem;
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

        /// <summary>
        /// Triggers an asyncronous sort operation. The sort engine first calculates the magnitude of the changes, and reports it to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Toolbar_SortMusic_Click(object sender, RoutedEventArgs e)
        {
            // TODO - obtain from settings file, make configurable
            var settings = new SortSettings
            {
                SortOrder = new List<MetaAttribute> {MetaAttribute.Artist, MetaAttribute.Album}
            };

            var sorter = new Sorter(new DirectoryInfo(_homeDir), settings);

            if (sorter.Actions.Count == 0) // Nothing to do! Notify and return.
            {
                MessageBox.Show(this,
                    String.Format(Resources["Dialog-SortLibrary-NoSort"].ToString(), sorter.UnsortableCount),
                    Resources["Dialog-SortLibrary-NoSortTitle"].ToString(), MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(this,
                String.Format(Resources["Dialog-SortLibrary-Confirm"].ToString(), sorter.MoveCount, sorter.DupCount,
                    sorter.UnsortableCount),
                Resources["Dialog-SortLibrary-Title"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await sorter.PerformSort();
                }
                catch (IOException ioe)
                {
                    // TODO - This should get localized and put in the application status bar (TBD)
                    MessageBox.Show("Encountered an error while sorting files: " + ioe.Message);
                }
            }
        }
        
    }
}
