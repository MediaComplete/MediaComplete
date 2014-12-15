﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Sorting;
using WinForms = System.Windows.Forms;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string Mp3FileFormat = "MP3 Files (*.mp3)|*.mp3";
        private const string FileDialogTitle = "Select Music File(s)";
        private string _homeDir;

        public MainWindow()
        {
            InitializeComponent();
            _homeDir = (string)Properties.Settings.Default["HomeDir"];
            var libraryDir = _homeDir;

            if (!_homeDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
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
            var fileDialog = new WinForms.OpenFileDialog
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
            var folderDialog = new WinForms.FolderBrowserDialog();
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
            var firstNode = new FolderTreeViewItem { Header = _homeDir, ParentItem = null, HasParent = false };

            SongTree.Items.Clear();

            var rootFiles = TreeViewBackend.GetFiles(_homeDir);
            var rootDirs = TreeViewBackend.GetDirectories(_homeDir);

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
                    await MusicIdentifier.IdentifySong(selection.FilePath());
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
            var treeViewItem = contextMenu.Parent as SongTreeViewItem;
            if (treeViewItem == null)
                return;

            try
            {
                await MusicIdentifier.IdentifySong(treeViewItem.GetPath());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // TODO status bar error message
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

            var sorter = new Sorter(new DirectoryInfo(_homeDir), settings);

            if (sorter.MoveActions.Count == 0) // Nothing to sort! Notify and return.
            {
                MessageBox.Show(this,
                    String.Format(Resources["Dialog-SortLibrary-NoSort"].ToString(), sorter.UnsortableCount),
                    Resources["Dialog-SortLibrary-NoSortTitle"].ToString(), MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(this,
                String.Format(Resources["Dialog-SortLibrary-Confirm"].ToString(), sorter.MoveActions.Count,
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