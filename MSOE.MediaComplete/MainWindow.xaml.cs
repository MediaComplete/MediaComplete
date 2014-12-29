using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private List<TextBox>_changedBoxes;
        private Settings _settings;

        public MainWindow()
        {
            InitializeComponent();

            _settings = new Settings();
            _changedBoxes = new List<TextBox>();

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
                await new Importer(SettingWrapper.GetHomeDir()).ImportFiles(files.Select(f => f.FullName).ToArray(), false);
            }
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

            ImportResults results;
            try
            {
                results = await new Importer(SettingWrapper.GetHomeDir()).ImportFiles(fileDialog.FileNames, true);
            }
            catch (InvalidImportException)
            {
                MessageBox.Show(this,
                    String.Format(Resources["Dialog-Import-Invalid-Message"].ToString()),
                    Resources["Dialog-Common-Error-Title"].ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (results.FailCount > 0)
            {
                MessageBox.Show(this, 
                    String.Format(Resources["Dialog-Import-ItemsFailed-Message"].ToString(), results.FailCount), 
                    Resources["Dialog-Common-Warning-Title"].ToString(), 
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new WinForms.FolderBrowserDialog();

            if (folderDialog.ShowDialog() != WinForms.DialogResult.OK) return;
            var selectedDir = folderDialog.SelectedPath;
            var results = await new Importer(SettingWrapper.GetHomeDir()).ImportDirectory(selectedDir, true);
            if (results.FailCount > 0)
            {
                MessageBox.Show(this,
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
            foreach (var rootChild in rootFiles.Where(rootChild => rootChild.Name.EndsWith(".mp3")))
            {
                SongTree.Items.Add(new SongTreeViewItem { Header = rootChild.Name, ParentItem = firstNode });
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
        private static FolderTreeViewItem PopulateFromFolder(DirectoryInfo dirInfo, ItemsControl songTree, FolderTreeViewItem parent)
        {
            var dirItem = new FolderTreeViewItem { Header = dirInfo.Name, ParentItem = parent };
            foreach (var dir in TreeViewBackend.GetDirectories(dirInfo))
            {
                dirItem.Children.Add(PopulateFromFolder(dir, songTree, dirItem));
            }

            foreach (var file in TreeViewBackend.GetFiles(dirInfo).Where(file => file.Name.EndsWith(".mp3")))
            {
                songTree.Items.Add(new SongTreeViewItem { Header = file.Name, ParentItem = dirItem });
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

        /// <summary>
        /// MouseClick Listener for the FolderTree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongTree_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopulateMetadataForm();
        }

        private void PopulateMetadataForm()
        {
            //Clear undo stack of each box
            var boxes = new TextBox[8];
            boxes[0] = SongTitle;
            boxes[1] = Album;
            boxes[2] = Artist;
            boxes[3] = SuppArtist;
            boxes[4] = Genre;
            boxes[5] = Track;
            boxes[6] = Year;
            boxes[7] = Rating;

            foreach (var box in boxes)
            {
                while (box.CanUndo)
                    box.Undo();
            }
            if (SongTree.SelectedItems.Count == 1)
            {
                var song = TagLib.File.Create(((SongTreeViewItem)SongTree.SelectedItems[0]).GetPath());
                SongTitle.Text = song.GetSongTitle();
                Album.Text = song.GetAlbum();
                Artist.Text = song.GetArtist();
                SuppArtist.Text = song.GetSupportingArtist();
                Genre.Text = song.GetGenre();
                Track.Text = song.GetTrack();
                Year.Text = song.GetYear();
                Rating.Text = song.GetRating();
            }
            else
            {
                var attributes = new Dictionary<MetaAttribute, string>
                {
                    {MetaAttribute.SongTitle, null},
                    {MetaAttribute.Album, null},
                    {MetaAttribute.Artist, null},
                    {MetaAttribute.SupportingArtist, null},
                    {MetaAttribute.Genre, null},
                    {MetaAttribute.TrackNumber, null},
                    {MetaAttribute.Year, null},
                    {MetaAttribute.Rating, null}
                };
                foreach (var song in from SongTreeViewItem item in SongTree.SelectedItems select TagLib.File.Create(item.GetPath()))
                {
                    switch (attributes[MetaAttribute.SongTitle])
                    {
                        case "-1":
                            break;
                        case null:
                            attributes[MetaAttribute.SongTitle] = song.GetSongTitle();
                            break;
                        default:
                            if (attributes[MetaAttribute.SongTitle] != song.GetSongTitle())
                            {
                                attributes[MetaAttribute.SongTitle] = "-1";
                            }
                            break;
                    }
                    switch (attributes[MetaAttribute.Album])
                    {
                        case "-1":
                            break;
                        case null:
                            attributes[MetaAttribute.Album] = song.GetAlbum();
                            break;
                        default:
                            if (attributes[MetaAttribute.Album] != song.GetAlbum())
                            {
                                attributes[MetaAttribute.Album] = "-1";
                            }
                            break;
                    }

                    switch (attributes[MetaAttribute.Artist])
                    {
                        case "-1":
                            break;
                        case null:
                            attributes[MetaAttribute.Artist] = song.GetArtist();
                            break;
                        default:
                            if (attributes[MetaAttribute.Artist] != song.GetArtist())
                            {
                                attributes[MetaAttribute.Artist] = "-1";
                            }
                            break;
                    }
                    switch (attributes[MetaAttribute.SupportingArtist])
                    {
                        case "-1":
                            break;
                        case null:
                            attributes[MetaAttribute.SupportingArtist] = song.GetSupportingArtist();
                            break;
                        default:
                            if (attributes[MetaAttribute.SupportingArtist] != song.GetSupportingArtist())
                            {
                                attributes[MetaAttribute.SupportingArtist] = "-1";
                            }
                            break;
                    }

                    switch (attributes[MetaAttribute.Genre])
                    {
                        case "-1":
                            break;
                        case null:
                            attributes[MetaAttribute.Genre] = song.GetGenre();
                            break;
                        default:
                            if (attributes[MetaAttribute.Genre] != song.GetGenre())
                            {
                                attributes[MetaAttribute.Genre] = "-1";
                            }
                            break;
                    }

                    switch (attributes[MetaAttribute.TrackNumber])
                    {
                        case "-1":
                            break;
                        case null:
                            attributes[MetaAttribute.TrackNumber] = song.GetTrack();
                            break;
                        default:
                            if (attributes[MetaAttribute.TrackNumber] != song.GetTrack())
                            {
                                attributes[MetaAttribute.TrackNumber] = "-1";
                            }
                            break;
                    }

                    switch (attributes[MetaAttribute.Year])
                    {
                        case "-1":
                            break;
                        case null:
                            attributes[MetaAttribute.Year] = song.GetYear();
                            break;
                        default:
                            if (attributes[MetaAttribute.Year] != song.GetYear())
                            {
                                attributes[MetaAttribute.Year] = "-1";
                            }
                            break;
                    }

                    switch (attributes[MetaAttribute.Rating])
                    {
                        case "-1":
                            break;
                        case null:
                            attributes[MetaAttribute.Rating] = song.GetRating();
                            break;
                        default:
                            if (attributes[MetaAttribute.Rating] != song.GetRating())
                            {
                                attributes[MetaAttribute.Rating] = "-1";
                            }
                            break;
                    }
                }
                SongTitle.Text = attributes[MetaAttribute.SongTitle] == "-1" ? "Various Songs" : attributes[MetaAttribute.SongTitle];
                Album.Text = attributes[MetaAttribute.Album] == "-1" ? "Various Albums" : attributes[MetaAttribute.Album];
                Artist.Text = attributes[MetaAttribute.Artist] == "-1" ? "Various Artists" : attributes[MetaAttribute.Artist];
                SuppArtist.Text = attributes[MetaAttribute.SupportingArtist] == "-1" ? "Various Artists" : attributes[MetaAttribute.SupportingArtist];
                Genre.Text = attributes[MetaAttribute.Genre] == "-1" ? "Various Genres" : attributes[MetaAttribute.Genre];
                Track.Text = attributes[MetaAttribute.TrackNumber] == "-1" ? "--" : attributes[MetaAttribute.TrackNumber];
                Year.Text = attributes[MetaAttribute.Year] == "-1" ? "Various Years" : attributes[MetaAttribute.Year];
                Rating.Text = attributes[MetaAttribute.Rating] == "-1" ? "Various Ratings" : attributes[MetaAttribute.Rating];
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
            foreach (var selection in from object item in SongTree.SelectedItems select item as SongTreeViewItem)
            {
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

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            if (SongTitle.IsReadOnly)
            {
                ToggleReadOnlyFields(false);
            }
            else if(_changedBoxes.Count > 0)
            {
                foreach (var changedBox in _changedBoxes)
                {
                    Console.Out.WriteLine(changedBox);
                    while (changedBox.CanUndo) { 
                        changedBox.Undo();
                    }
                    changedBox.Redo();
                    changedBox.LockCurrentUndoUnit();
                }
                _changedBoxes.Clear();
                ToggleReadOnlyFields(true);
            }
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (SongTitle.IsReadOnly) return;
            ToggleReadOnlyFields(true);
            var attributes = new Dictionary<MetaAttribute, TextBox>
                {
                    {MetaAttribute.SongTitle, SongTitle},
                    {MetaAttribute.Album, Album},
                    {MetaAttribute.Artist, Artist},
                    {MetaAttribute.SupportingArtist, SuppArtist},
                    {MetaAttribute.Genre, Genre},
                    {MetaAttribute.TrackNumber, Track},
                    {MetaAttribute.Year, Year},
                    {MetaAttribute.Rating, Rating}
                };
            foreach (var song in from SongTreeViewItem item in SongTree.SelectedItems select TagLib.File.Create(item.GetPath()))
            {
                foreach (var changedBox in _changedBoxes)
                {
                    if (changedBox.Equals(SongTitle))
                    {
                        song.SetSongTitle(changedBox.Text);
                    }
                    else if (changedBox.Equals(Album))
                    {
                        song.SetAlbum(changedBox.Text);
                    }
                    else if (changedBox.Equals(Artist))
                    {
                        song.SetArtist(changedBox.Text);
                    }
                    else if (changedBox.Equals(Genre))
                    {
                        song.SetGenre(changedBox.Text);
                    }
                    else if (changedBox.Equals(Track))
                    {
                        song.SetTrack(changedBox.Text);   
                    }
                    else if (changedBox.Equals(Year))
                    {
                        song.SetYear(changedBox.Text);
                    }
                    else if (changedBox.Equals(Rating))
                    {
                        song.SetRating(changedBox.Text);
                    }
                    else if (changedBox.Equals(SuppArtist))
                    {
                        song.SetSupportingArtists(changedBox.Text);
                    }
                }
            }
            _changedBoxes.Clear();
        }

        private void ToggleReadOnlyFields(Boolean toggle)
        {
            SongTitle.IsReadOnly = toggle;
            Album.IsReadOnly = toggle;
            SuppArtist.IsReadOnly = toggle;
            Artist.IsReadOnly = toggle;
            Genre.IsReadOnly = toggle;
            Year.IsReadOnly = toggle;
            Rating.IsReadOnly = toggle;
            Track.IsReadOnly = toggle;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_changedBoxes.Contains((TextBox) e.OriginalSource) && !SongTitle.IsReadOnly)
                _changedBoxes.Add((TextBox) e.OriginalSource);
        }
    }
}
