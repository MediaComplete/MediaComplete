using System;

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MSOE.MediaComplete.Lib;
using System.Windows;
using System.Windows.Input;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib.Sorting;
using System.Windows.Controls;
using Application = System.Windows.Application;

using WinForms = System.Windows.Forms;

namespace MSOE.MediaComplete
{
    /// <summary>

    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Settings _settings;

        private readonly Importer _importer;

        public MainWindow()
        {
            InitializeComponent();

            _settings = new Settings();

            var homeDir = SettingWrapper.GetHomeDir() ??
                          Path.GetPathRoot(Environment.SystemDirectory);
            StatusBarHandler.Instance.RaiseStatusBarEvent += HandleStatusBarChangeEvent;
            if (!homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                homeDir += Path.DirectorySeparatorChar;
            }

            if (SettingWrapper.GetIsPolling())
            {
                Polling.Instance.TimeInMinutes = SettingWrapper.GetPollingTime();
                Polling.Instance.Start();
            }
            Polling.InboxFilesDetected += ImportFromInbox;
            Directory.CreateDirectory(homeDir);

            _importer = new Importer(SettingWrapper.GetHomeDir());
            InitTreeView();
        }

        private void HandleStatusBarChangeEvent(string message, StatusBarHandler.StatusIcon icon)
        {
            Dispatcher.Invoke(() =>
            {
                StatusMessage.Text = (message.Length == 0) ? "" : Resources[message].ToString();
                var sourceUri = new Uri("./Resources/" + icon + ".png", UriKind.Relative);
                StatusIcon.Source = new BitmapImage(sourceUri);
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private async void ImportFromInbox(IEnumerable<FileInfo> files)
        {
            if (SettingWrapper.GetShowInputDialog())
            {
                Dispatcher.BeginInvoke(new Action(() => InboxImportDialog.Prompt(this, files)));
            }
            else
            {
                await _importer.ImportFiles(files.Select(f => f.FullName).ToArray(), false);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_settings.IsLoaded) return;
            _settings = new Settings();
            _settings.ShowDialog();
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

            if (fileDialog.ShowDialog() != WinForms.DialogResult.OK) return;
            await Task.Run(() => _importer.ImportFiles(fileDialog.FileNames, true));
            RefreshTreeView();
        }

        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedDir = folderDialog.SelectedPath;
                await Task.Run(() => _importer.ImportDirectory(selectedDir, true));
            }
            RefreshTreeView();
        }

        /// <summary>
        /// populates the treeviews with all valid elements within the home directory
        /// </summary>
        public void RefreshTreeView()
        {
            //Create Parent node
            var firstNode = new FolderTreeViewItem { Header = SettingWrapper.GetHomeDir(), ParentItem = null, HasParent = false };

            SongTree.Items.Clear();

            var rootFiles = TreeViewBackend.GetFiles();
            var rootDirs = TreeViewBackend.GetDirectories();

            //For each folder in the root Directory
            foreach (var rootChild in rootDirs)
            {   
                //add each child to the root folder
                firstNode.Children.Add(PopulateFromFolder(rootChild, SongTree, firstNode));
            }
            foreach (var rootChild in rootFiles)
            {
                if (rootChild.Name.EndsWith(".mp3"))
                {
                    SongTree.Items.Add(new SongTreeViewItem { Header = rootChild.Name, ParentItem = firstNode });
                }
            }

            DataContext = firstNode;
        }

        private void InitTreeView()
        {
            RefreshTreeView();

            var watcher = new FileSystemWatcher(SettingWrapper.GetHomeDir())
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;

            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Recursively populates foldertree and songtree with elements
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="songTree"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static FolderTreeViewItem PopulateFromFolder(DirectoryInfo dirInfo, TreeViewEx songTree, FolderTreeViewItem parent)
        {
            var dirItem = new FolderTreeViewItem { Header = dirInfo.Name, ParentItem = parent };
            foreach (var dir in TreeViewBackend.GetDirectories(dirInfo))
            {
                dirItem.Children.Add(PopulateFromFolder(dir, songTree, dirItem));
            }

            foreach (var file in TreeViewBackend.GetFiles(dirInfo))
            {
                if (file.Name.EndsWith(".mp3"))
                {
                    songTree.Items.Add(new SongTreeViewItem { Header = file.Name, ParentItem = dirItem });
                }
            }
            return dirItem;
        }

        /// <summary>
        /// MouseClick Listener for the FolderTree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTree_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FolderTree.SelectedItems != null && FolderTree.SelectedItems.Count > 0)
            {
                SongTree.Items.Clear();
                foreach (var folder in FolderTree.SelectedItems)
                {
                    var item = (FolderTreeViewItem)folder;
                    var rootDirInfo = new DirectoryInfo((item.GetPath()));
                    if (!ContainsParent(item))
                    {
                        PopulateFromFolder(rootDirInfo, SongTree, item);
                    }
                }
            }
            else
            {
                RefreshTreeView();
            }
        }

        private Boolean ContainsParent(FolderTreeViewItem folder)
        {
            if (!folder.HasParent)
            {
                return false;
            }
            return (FolderTree.SelectedItems.Contains(folder.ParentItem) || ContainsParent(folder.ParentItem));
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
            // TODO mass ID of multi-selected songs or folders
            foreach (var item in SongTree.SelectedItems)
            {
                var selection = item as SongTreeViewItem;
                try
                {
                    if (selection != null) { 
                        await MusicIdentifier.IdentifySong(selection.GetPath());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // TODO status bar error message
                }
            }
        }
        private async void ContextMenu_AutoIDMusic_Click(object sender, RoutedEventArgs e)
        {
            // Access the targetted song 
            // TODO mass ID of multi-selected songs
            // TODO provide this context menu item for folders
            var menuItem = sender as MenuItem;
            if (menuItem == null)
                return;
            var contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu == null)
                return;
            foreach (var item in SongTree.SelectedItems)
            {
                try
                {
                    await MusicIdentifier.IdentifySong(((SongTreeViewItem)item).GetPath());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // TODO status bar error message
                }
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
                SortOrder = new List<MetaAttribute> { MetaAttribute.Artist, MetaAttribute.Album }
            };

            var sorter = new Sorter(new DirectoryInfo(SettingWrapper.GetHomeDir()), settings);

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

            if (result != MessageBoxResult.Yes) return;
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