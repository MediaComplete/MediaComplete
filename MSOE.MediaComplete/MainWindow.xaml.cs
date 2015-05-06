using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Files;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Playing;
using MSOE.MediaComplete.Lib.Playlists;
using MSOE.MediaComplete.Lib.Sorting;
using NAudio.Wave;
using Queue = MSOE.MediaComplete.Lib.Background.Queue;
using WinForms = System.Windows.Forms;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Properties

        /// <summary>
        /// The root of the library TreeView strucutre.
        /// </summary>
        public FolderTreeViewItem RootLibraryFolderItem
        {
            get { return _rootLibItem; } 
        }
        private readonly FolderTreeViewItem _rootLibItem = new FolderTreeViewItem { Header = SettingWrapper.MusicDir, IsSelected = true };

        /// <summary>
        /// Contains the songs in the middle view. Filtered based on what's happening in the left pane.
        /// </summary>
        public CollectionViewSource Songs { get { return _songs; } }
        private readonly CollectionViewSource _songs = new CollectionViewSource { Source = new ObservableCollection<SongListItem>(), IsLiveFilteringRequested = true };
        
        /// <summary>
        /// Contains the songs in the middle view. Filtered based on what's happening in the left pane.
        /// </summary>
        public CollectionViewSource PlaylistSongs { get { return _playlistSongs; } }
        private readonly CollectionViewSource _playlistSongs = new CollectionViewSource { Source = new ObservableCollection<SongListItem>() };

        /// <summary>
        /// Private abstraction of the file system
        /// </summary>
        private readonly IFileManager _fileManager = FileManager.Instance;
        #endregion

        #region Construction
        /// <summary>
        /// And so it begins.
        /// </summary>
        public MainWindow()
        {
            _fileManager.Initialize(SettingWrapper.MusicDir);
            InitializeComponent();
            InitUi();
            InitEvents();
            InitTreeView();
            InitPlayer();
        }

        /// <summary>
        /// Setup special UI elements
        /// </summary>
        public static void InitUi()
        {
            var dictUri = new Uri(SettingWrapper.Layout, UriKind.Relative);
            var resourceDict = Application.LoadComponent(dictUri) as ResourceDictionary;
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }

        /// <summary>
        /// Setup application wide events
        /// </summary>
        private void InitEvents()
        {
            StatusBarHandler.Instance.RaiseStatusBarEvent += HandleStatusBarChangeEvent;
            Polling.InboxFilesDetected += ImportFromInboxAsync;
            // ReSharper disable once ObjectCreationAsStatement
            new Sorter(_fileManager, null); // Run static constructor
            // ReSharper disable once UnusedVariable
            var tmp = Polling.Instance;  // Run singleton constructor
        }

        /// <summary>
        /// Setup the song library
        /// </summary>
        private void InitTreeView()
        {
            // Set the library sorter
            var songsView = Songs.View as ListCollectionView;
            if (songsView != null)
                songsView.CustomSort = new PathSorter();

            // Set the library filter
            Songs.Filter += LibrarySongFilter;

            // Initial population
            SongCreated(_fileManager.GetAllSongs()); 

            // Subscribe to file system updates
            _fileManager.SongChanged += SongChanged;
            _fileManager.SongCreated += SongCreated;
            _fileManager.SongDeleted += SongDeleted;
            _fileManager.SongRenamed += SongRenamed;
        }
        #endregion

        #region User triggered events

        /// <summary>
        /// Open the Settings window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolbarSettings_Click(object sender, RoutedEventArgs e)
        {
            new Settings { Owner = this }.ShowDialog();
        }

        /// <summary>
        /// Open a file selection dialog for importing, and do the import.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddFile_ClickAsync(object sender, RoutedEventArgs e)
        {
            var fileDialog = new WinForms.OpenFileDialog
            {
                Filter =
                    Resources["Dialog-AddFile-MusicFilter"] + "" + Constants.FileDialogFilterStringSeparator + string.Join<string>(";", Constants.MusicFileExtensions.Select(s => Constants.Wildcard + s)) + Constants.FileDialogFilterStringSeparator +
                    Resources["Dialog-AddFile-Mp3Filter"] + "" + Constants.FileDialogFilterStringSeparator + Constants.Wildcard + Constants.MusicFileExtensions[0] + Constants.FileDialogFilterStringSeparator +
                    Resources["Dialog-AddFile-WmaFilter"] + "" + Constants.FileDialogFilterStringSeparator + Constants.Wildcard + Constants.MusicFileExtensions[1],
                InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory),
                Title = Resources["Dialog-AddFile-Title"].ToString(),
                Multiselect = true
            };

            if (fileDialog.ShowDialog() != WinForms.DialogResult.OK) return;

            ImportResults results;
            try
            {
                results = await new Importer(_fileManager).ImportFilesAsync(fileDialog.FileNames.Select(p => new SongPath(p)).ToList(), SettingWrapper.ShouldRemoveOnImport);
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

        /// <summary>
        /// Open a folder selection dialog, and import the results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddFolder_ClickAsync(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();

            if (folderDialog.ShowDialog() != WinForms.DialogResult.OK) return;
            var selectedDir = folderDialog.SelectedPath;
            var files = new DirectoryInfo(selectedDir).EnumerateFiles("*", SearchOption.AllDirectories).GetMusicFiles().Select(x => new SongPath(x.FullName));
            var results = await new Importer(_fileManager).ImportDirectoryAsync(files, SettingWrapper.ShouldRemoveOnImport);
            if (results.FailCount > 0)
            {
                MessageBox.Show(Application.Current.MainWindow,
                    String.Format(Resources["Dialog-Import-ItemsFailed-Message"].ToString(), results.FailCount),
                    Resources["Dialog-Common-Warning-Title"].ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Updates the song list based on the folder selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTree_OnSelectionChanged(object sender, EventArgs e)
        {
            if (!IsLoaded)
                return;

            FormCheck();
            if (FolderTree.SelectedItems != null)
            {
                RootLibraryFolderItem.IsSelected = FolderTree.SelectedItems.Count == 0;
                Songs.View.Refresh();
            }
            ClearDetailPane();
        }

        /// <summary>
        /// Updates the metadata form based on the song list selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FormCheck();
            if (AllSongs().Any())
                PopulateMetadataForm();
            else
                ClearDetailPane();
        }

        /// <summary>
        /// Perform an auto-metadata restoration of the selected songs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toolbar_AutoIDMusic_ClickAsync(object sender, RoutedEventArgs e)
        {
            // TODO (MC-45) mass ID of folders
            Queue.Inst.Add(new MusicIdentifier(SelectedSongs().Select(l => l.Data).OfType<LocalSong>().ToList()));
        }

        /// <summary>
        /// Performs an auto-metadata restoration of the selected songs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AutoIDMusic_ClickAsync(object sender, RoutedEventArgs e)
        {
            // TODO (MC-45) mass ID of folders
            Queue.Inst.Add(new MusicIdentifier(SelectedSongs().Select(l => l.Data).OfType<LocalSong>().ToList()));
        }

        /// <summary>
        /// Triggers an asyncronous sort operation. The sort engine first calculates the magnitude of the changes, and reports it to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Toolbar_SortMusic_ClickAsync(object sender, RoutedEventArgs e)
        {
            var settings = new SortSettings
            {
                SortOrder = SettingWrapper.SortOrder,
                Files = _fileManager.GetAllSongs().Select(x => x.SongPath),
            };

            var sorter = new Sorter(_fileManager, settings);
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

        /// <summary>
        /// Updates the active list reference based on the new tab. Actual list hiding and 
        /// showing is done via bindings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftFrame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistTab.IsSelected)
            {
                NowPlayingItem.IsSelected = true;
                // Manually fire this, NowPlayingItem.IsSelected won't do the job if that's already selected
                PlaylistTree_SelectionChanged(null, null);
            }
            ClearDetailPane();
        }

        /// <summary>
        /// Update the list of songs based on the selected playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaylistTree_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var playlistSongs = (ObservableCollection<SongListItem>)PlaylistSongs.Source;
            var list = NowPlayingItem.IsSelected ? NowPlaying.Inst.Playlist : (Playlist)PlaylistTree.SelectedItem;

            playlistSongs.Clear();
            list.Songs.ForEach(s => playlistSongs.Add(new SongListItem { Content = s, Data = s }));

            // If now-playing, highlight the current song
            if (NowPlayingItem.IsSelected && NowPlaying.Inst.Index > -1 && !_player.PlaybackState.Equals(PlaybackState.Stopped))
            {
                var item = playlistSongs[NowPlaying.Inst.Index];
                item.IsPlaying = true;
                PlaylistSongList.ScrollIntoView(item);
            }

            ClearDetailPane();
        }

        #endregion

        #region Internally triggered events
        /// <summary>
        /// Updates the song items in the list. Just calls rename since both need to remove and re-add
        /// </summary>
        /// <param name="songs">The updated songs</param>
        private void SongChanged(IEnumerable<LocalSong> songs)
        {
            SongRenamed(songs.Select(s => new Tuple<LocalSong, LocalSong>(s, s)));
        }

        /// <summary>
        /// Removes songs from the list
        /// </summary>
        /// <param name="songs">The songs to remove</param>
        private void SongDeleted(IEnumerable<LocalSong> songs)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var song in songs.Select(song => ((ObservableCollection<SongListItem>)Songs.Source).FirstOrDefault(x => x.Data.Id.Equals(song.Id))).Where(song => song != null))
                {
                    ((ObservableCollection<SongListItem>)Songs.Source).Remove(song);
                    // Roll up the empty folders
                    var parent = song.ParentItem;
                    while (_fileManager.DirectoryEmpty(new DirectoryPath(parent.GetPath())) && parent.ParentItem != null)
                    {
                        parent.ParentItem.Children.Remove(parent);
                        parent = parent.ParentItem;
                    }
                }
            });
        }

        /// <summary>
        /// Adds new songs into the list.
        /// </summary>
        /// <param name="songs">The new songs</param>
        private void SongCreated(IEnumerable<LocalSong> songs)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var song in songs)
                {
                    var parent = AddFolderTreeViewItems(song.SongPath.Directory);
                    ((ObservableCollection<SongListItem>)Songs.Source).Add(new SongListItem { Content = song.Name, ParentItem = parent, Data = song });
                }
            });
        }

        /// <summary>
        /// Adds and removes the song so it's place in the list is updated.
        /// </summary>
        /// <param name="songs">A tuple of old and new songs (may contain data edits)</param>
        private void SongRenamed(IEnumerable<Tuple<LocalSong, LocalSong>> songs)
        {
            SongDeleted(songs.Select(t => t.Item1));
            SongCreated(songs.Select(t => t.Item2));
        }

        /// <summary>
        /// Filters the songs based on whether one of their ancestors in the library treeview is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void LibrarySongFilter(object sender, FilterEventArgs e)
        {
            var song = e.Item as SongListItem;
            e.Accepted = song != null && song.ParentItem.IsSelectedRecursive();
        }

        /// <summary>
        /// Catch status updates from the event and display them on the status bar.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="message"></param>
        /// <param name="icon"></param>
        /// <param name="extraArgs"></param>
        private void HandleStatusBarChangeEvent(string format, string message, StatusBarHandler.StatusIcon icon, params object[] extraArgs)
        {
            Dispatcher.Invoke(() =>
            {
                var args = (new[] { message == null ? "" : Resources[message] }).Concat(extraArgs);
                StatusMessage.Text = String.Format(format, args.ToArray());
                var sourceUri = new Uri("./Resources/" + icon + ".png", UriKind.Relative);
                StatusIcon.Source = new BitmapImage(sourceUri);
            });
        }

        /// <summary>
        /// Triggered by the inbox file polling. Prompts the user, or just automagically imports.
        /// </summary>
        /// <param name="files">Newly discovered files.</param>
        private async void ImportFromInboxAsync(IEnumerable<SongPath> files)
        {
            if (SettingWrapper.ShowInputDialog)
            {
                Dispatcher.BeginInvoke(new Action(() => InboxImportDialog.Prompt(this, files)));
            }
            else
            {
                await new Importer(_fileManager).ImportFilesAsync(files, true);
            }
        }
        #endregion

        #region Private helpers

        /// <summary>
        /// Helper methods to return the selected song items from the currently visible list.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SongListItem> SelectedSongs()
        {
            var currentSongs = (ObservableCollection<SongListItem>)(PlaylistTab.IsSelected ? PlaylistSongs : Songs).Source;
            return currentSongs.Where(x => x.IsSelected && x.IsVisible);
        }

        /// <summary>
        /// Helper methods to return all song items from the currently visible list.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SongListItem> AllSongs()
        {
            var currentSongs = (ObservableCollection<SongListItem>)(PlaylistTab.IsSelected ? PlaylistSongs : Songs).Source;
            return currentSongs.Where(s => s.IsVisible);
        }

        /// <summary>
        /// Returns the first index of a selected song in the currently visible list.
        /// </summary>
        /// <returns></returns>
        private int SelectedSongIndex()
        {
            var currentSongs = (ObservableCollection<SongListItem>)(PlaylistTab.IsSelected ? PlaylistSongs : Songs).Source;
            return currentSongs.IndexOf(currentSongs.FirstOrDefault(s => s.IsSelected));
        }

        /// <summary>
        /// Builds out the treeview for the given folder path, as needed.
        /// </summary>
        /// <param name="path">The path to add to the treeview</param>
        /// <returns>The last folder treeview item in the heirarchy</returns>
        private FolderTreeViewItem AddFolderTreeViewItems(DirectoryPath path)
        {
            // First see if it's in the root of the library
            if (path.FullPath.Length <= SettingWrapper.MusicDir.FullPath.Length)
                return _rootLibItem;
            
            // First, lop off everything up to the music dir
            var pathStr = path.FullPath.Substring(SettingWrapper.MusicDir.FullPath.Length);
            // Now break into individual "folder" names
            var folderNames = pathStr.Split(Path.DirectorySeparatorChar);

            // Now fill out the treeview with the folder names
            return folderNames.Aggregate(_rootLibItem, 
                (parentTreeViewItem, folderName) => parentTreeViewItem.Children.FirstOrDefault(t => 
                    (string) t.Header == folderName) ?? new FolderTreeViewItem { Header = folderName, ParentItem = parentTreeViewItem });
        }

        /// <summary>
        /// Used for mapping the sort order to the song list's sort binding.
        /// </summary>
        private class PathSorter : IComparer
        {
            public int Compare(object o1, object o2)
            {
                var x = o1 as SongListItem;
                var y = o2 as SongListItem;

                if (x == null || y == null)
                {
                    throw new ArgumentException();
                }

                // Get the string paths of the parent folders, minus the music dir (speeds up a bit)
                var xPath = x.ParentItem.GetPath().Substring(SettingWrapper.MusicDir.FullPath.Length);
                var yPath = y.ParentItem.GetPath().Substring(SettingWrapper.MusicDir.FullPath.Length);

                // Compare by path
                var pathDiff = String.Compare(xPath, yPath, StringComparison.Ordinal);

                // Compare by header (filename) if the paths are equal
                return pathDiff != 0 ? pathDiff : String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
            }
        }

        #endregion
    }
}
