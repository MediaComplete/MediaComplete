using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using System.Threading;
using WinForms = System.Windows.Forms;
using System.Windows.Media.Imaging;
using MSOE.MediaComplete.Lib;
using System.Windows;
using System.Windows.Input;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib.Sorting;
using System.Windows.Controls;
using Application = System.Windows.Application;
using System.Globalization;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<TextBox>_changedBoxes;
        private Settings _settings;
        private readonly Timer _refreshTimer;

        public MainWindow()
        {
            InitializeComponent();

            _settings = new Settings();
            _changedBoxes = new List<TextBox>();

            var homeDir = SettingWrapper.MusicDir ??
                          Path.GetPathRoot(Environment.SystemDirectory);

            ChangeSortMusic();
            StatusBarHandler.Instance.RaiseStatusBarEvent += HandleStatusBarChangeEvent;

            if (!homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal))
            {
                homeDir += Path.DirectorySeparatorChar;
            }
            
            var dictUri  = new Uri(SettingWrapper.Layout, UriKind.Relative);
            
            var resourceDict = Application.LoadComponent(dictUri) as ResourceDictionary;
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);

            if (SettingWrapper.IsPolling)
            {
                Polling.Instance.TimeInMinutes = SettingWrapper.PollingTime;

                Polling.Instance.Start();
            }
            Directory.CreateDirectory(homeDir);
            _refreshTimer = new Timer(TimerProc);
            
            
            InitEvents();

            InitTreeView();

            InitPlayer();
        }

        private void InitEvents()
        {
            Polling.InboxFilesDetected += ImportFromInboxAsync;
            SettingWrapper.RaiseSettingEvent += HandleSettingEvent;
            // ReSharper disable once ObjectCreationAsStatement
            new Sorter(null);
        }

        private void HandleStatusBarChangeEvent(string format, string message, StatusBarHandler.StatusIcon icon, params object[] extraArgs)
        {
            Dispatcher.Invoke(() =>
            {
                var args = (new[] {message == null ? "" : Resources[message]}).Concat(extraArgs);
                StatusMessage.Text = String.Format(format, args.ToArray());
                var sourceUri = new Uri("./Resources/" + icon + ".png", UriKind.Relative);
                StatusIcon.Source = new BitmapImage(sourceUri);
            });
        }

        private void HandleSettingEvent()
        {
            ChangeSortMusic();
        }

        private void ChangeSortMusic()
        {
            var content = SettingWrapper.IsSorting ? Resources["Toolbar-SortMusic-Tooltip"].ToString() : Resources["Toolbar-SortMusicDisabled-Tooltip"].ToString();
            SortMusic.ToolTip = content;
            SortMusic.IsEnabled = SettingWrapper.IsSorting;
        }

        private async void ImportFromInboxAsync(IEnumerable<FileInfo> files)
        {
            if (SettingWrapper.ShowInputDialog)
            {
                Dispatcher.BeginInvoke(new Action(() => InboxImportDialog.Prompt(Application.Current.MainWindow, files)));
            }
            else
            {
                await new Importer(SettingWrapper.MusicDir).ImportFilesAsync(files, true);
            }
        }
        

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_settings.IsLoaded) return;
            _settings = new Settings();
            _settings.ShowDialog();
            RefreshTreeView();
        }

        private async void AddFile_ClickAsync(object sender, RoutedEventArgs e)
        {
            var fileDialog = new WinForms.OpenFileDialog
            {
                Filter =
                    Resources["Dialog-AddFile-MusicFilter"] + "" + Lib.Constants.FileDialogFilterStringSeparator + string.Join<string>(";",Lib.Constants.MusicFileExtensions.Select(s => Lib.Constants.Wildcard+s)) + Lib.Constants.FileDialogFilterStringSeparator +
                    Resources["Dialog-AddFile-Mp3Filter"] + "" + Lib.Constants.FileDialogFilterStringSeparator + Lib.Constants.Wildcard + Lib.Constants.MusicFileExtensions[0] + Lib.Constants.FileDialogFilterStringSeparator +
                    Resources["Dialog-AddFile-WmaFilter"] + "" + Lib.Constants.FileDialogFilterStringSeparator + Lib.Constants.Wildcard + Lib.Constants.MusicFileExtensions[1],
                    InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory),
                    Title = Resources["Dialog-AddFile-Title"].ToString(),
                    Multiselect = true
            };

            if (fileDialog.ShowDialog() != WinForms.DialogResult.OK) return;

            ImportResults results;
            try
            {
                results = await new Importer(SettingWrapper.MusicDir).ImportFilesAsync(fileDialog.FileNames.Select(p => new FileInfo(p)).ToList(), SettingWrapper.ShouldRemoveOnImport);
            }
            catch (InvalidImportException)
            {
                MessageBox.Show(Application.Current.MainWindow,
                    String.Format(Resources["Dialog-Import-Invalid-Message"].ToString()),
                    Resources["Dialog-Common-Error-Title"].ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (results.FailCount > 0)
            {
                MessageBox.Show(Application.Current.MainWindow, 
                    String.Format(Resources["Dialog-Import-ItemsFailed-Message"].ToString(), results.FailCount), 
                    Resources["Dialog-Common-Warning-Title"].ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private async void AddFolder_ClickAsync(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();

            if (folderDialog.ShowDialog() != WinForms.DialogResult.OK) return;
            var selectedDir = folderDialog.SelectedPath;

            var results = await new Importer(SettingWrapper.MusicDir).ImportDirectoryAsync(selectedDir, SettingWrapper.ShouldRemoveOnImport);
            if (results.FailCount > 0)
            {
                MessageBox.Show(Application.Current.MainWindow,
                    String.Format(Resources["Dialog-Import-ItemsFailed-Message"].ToString(), results.FailCount),
                    Resources["Dialog-Common-Warning-Title"].ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            RefreshTreeView();
        }

        /// <summary>
        /// populates the treeviews with all valid elements within the home directory
        /// </summary>
        public void RefreshTreeView()
        {
            //Create Parent node
            var firstNode = new FolderTreeViewItem { Header = SettingWrapper.MusicDir, ParentItem = null};

            SongTree.Items.Clear();
            var rootFiles = new DirectoryInfo(SettingWrapper.MusicDir).GetFilesOrCreateDir();
            var rootDirs = new DirectoryInfo(SettingWrapper.MusicDir).GetDirectories();

            //For each folder in the root Directory
            foreach (var rootChild in rootDirs)
            {   
                //add each child to the root folder
                firstNode.Children.Add(PopulateFromFolder(rootChild, SongTree, firstNode));
            }

            foreach (var rootChild in rootFiles.GetMusicFiles())
            {
                SongTree.Items.Add(new SongTreeViewItem { Header = rootChild.Name, ParentItem = firstNode });
            }

            DataContext = firstNode;
        }

        private void InitTreeView()
        {
            RefreshTreeView();

            var watcher = new FileSystemWatcher(SettingWrapper.MusicDir)
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
        private static FolderTreeViewItem PopulateFromFolder(DirectoryInfo dirInfo, ItemsControl songTree, FolderTreeViewItem parent)
        {
            var dirItem = new FolderTreeViewItem { Header = dirInfo.Name, ParentItem = parent };
            foreach (var dir in dirInfo.GetDirectories())
            {
                dirItem.Children.Add(PopulateFromFolder(dir, songTree, dirItem));
            }
            
            foreach (var file in dirInfo.GetFilesOrCreateDir().GetMusicFiles())
            {
                songTree.Items.Add(new SongTreeViewItem { Header = file.Name, ParentItem = dirItem });
            }
            return dirItem;
        }

        private static void PopulateSongTree(DirectoryInfo dirInfo, ItemsControl songTree, FolderTreeViewItem parent, bool root)
        {
            var dirItem = root ? parent : new FolderTreeViewItem { Header = dirInfo.Name, ParentItem = parent };
            foreach (var dir in dirInfo.GetDirectories())
            {
                PopulateSongTree(dir, songTree, dirItem, false);
            }

            foreach (var x in dirInfo.GetFilesOrCreateDir().GetMusicFiles().Select(file => new SongTreeViewItem { Header = file.Name, ParentItem = dirItem }))
            {
                songTree.Items.Add(x);
            }
        }

        /// <summary>
        /// MouseClick Listener for the FolderTree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTree_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            FormCheck();
            if (FolderTree.SelectedItems != null && FolderTree.SelectedItems.Count > 0)
            {
                SongTree.Items.Clear();
                foreach (var folder in FolderTree.SelectedItems)
                {
                    //current file
                    var item = (FolderTreeViewItem)folder;
                    //dirinfo of current file
                    var dirInfo = new DirectoryInfo((item.GetPath()));
                    if (!ContainsParent(item))
                    {
                        PopulateSongTree(dirInfo, SongTree, item, true);
                    }
                }
            }
            else
            {
                RefreshTreeView();
            }
            ClearDetailPane();
        }

        /// <summary>
        /// MouseClick Listener for the FolderTree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongTree_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            FormCheck();
            if(SongTree.SelectedItems.Count > 0)
                PopulateMetadataForm();
            else
                ClearDetailPane();
        }

        private Boolean ContainsParent(FolderTreeViewItem folder)
        {
            if (folder.ParentItem==null)
            {
                return false;
            }
            return (FolderTree.SelectedItems.Contains(folder.ParentItem) || ContainsParent(folder.ParentItem));
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            _refreshTimer.Change(500, Timeout.Infinite);
        }
        
        private static void TimerProc(object state)
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

        private async void Toolbar_AutoIDMusic_ClickAsync(object sender, RoutedEventArgs e)
        {
            // TODO (MC-45) mass ID of multi-selected songs and folders
            foreach (var selection in from object item in SongTree.SelectedItems select item as SongTreeViewItem)
            {
                try
                {
                    if (selection == null) continue;
                    await MusicIdentifier.IdentifySongAsync(selection.GetPath());
                }
                catch (Exception ex)
                {
                    // TODO (MC-125) Logging
                    StatusBarHandler.Instance.ChangeStatusBarMessage(
                        String.Format(Resources["MusicIdentification-Error"].ToString(), ex.Message),
                        StatusBarHandler.StatusIcon.Error);
                }
            }
        }

        private async void ContextMenu_AutoIDMusic_ClickAsync(object sender, RoutedEventArgs e)
        {
            // Access the targetted song 
            // TODO (MC-45) mass ID of multi-selected songs and folders
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
                    await MusicIdentifier.IdentifySongAsync(((SongTreeViewItem)item).GetPath());
                }
                catch (Exception ex)
                {
                    // TODO (MC-125) Logging
                    StatusBarHandler.Instance.ChangeStatusBarMessage(
                        String.Format(Resources["MusicIdentification-Error"].ToString(), ex.Message), 
                        StatusBarHandler.StatusIcon.Error);
                }
            }
            
        }

        /// <summary>
        /// Triggers an asyncronous sort operation. The sort engine first calculates the magnitude of the changes, and reports it to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Toolbar_SortMusic_ClickAsync(object sender, RoutedEventArgs e)
        {
            // TODO (MC-43) obtain from settings file, make configurable
            var settings = new SortSettings
            {
                SortOrder = SettingWrapper.SortOrder,
                Root = new DirectoryInfo(SettingWrapper.MusicDir),
                Files =  new DirectoryInfo(SettingWrapper.MusicDir).EnumerateFiles("*", SearchOption.AllDirectories)
                    .GetMusicFiles()
            };

            var sorter = new Sorter(settings);
            await sorter.CalculateActionsAsync();    

            if (sorter.Actions.Count == 0) // Nothing to do! Notify and return.
            {
                MessageBox.Show(Application.Current.MainWindow,
                    String.Format(Resources["Dialog-SortLibrary-NoSort"].ToString(), sorter.UnsortableCount),
                    Resources["Dialog-SortLibrary-NoSortTitle"].ToString(), MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(Application.Current.MainWindow,
                String.Format(Resources["Dialog-SortLibrary-Confirm"].ToString(), sorter.MoveCount, sorter.DupCount,
                    sorter.UnsortableCount),
                Resources["Dialog-SortLibrary-Title"].ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;
            
            sorter.PerformSort();
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_changedBoxes.Contains((TextBox) sender) || SongTitle.IsReadOnly) return;
            _changedBoxes.Add((TextBox)sender);
            StatusBarHandler.Instance.ChangeStatusBarMessage("", StatusBarHandler.StatusIcon.None);
        }
    }
}
