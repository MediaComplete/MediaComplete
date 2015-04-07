using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib.Playing;
using MSOE.MediaComplete.Lib.Playlists;
using MSOE.MediaComplete.Lib.Songs;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Handles logic in the Playlists treeview tab.
    /// Contains methods and properties related to playlists and playlist handling.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Bindable list of Playlists. Gets used in context menus and elsewhere.
        /// </summary>
        public ObservableCollection<Playlist> Playlists { get { return _playlists; } }
        private readonly ObservableCollection<Playlist> _playlists = new ObservableCollection<Playlist>(PlaylistService.GetAllPlaylists());

        #region Event handlers

        /// <summary>
        /// Creates a new playlist and populates it based on the selected folders. If no folder is 
        /// selected, no action is taken.
        /// 
        /// If there is only one selected folder, this is used as the playlist name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddFoldersToNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItems.Count <= 0)
                return;

            Playlist list = null;
            if (FolderTree.SelectedItems.Count == 1)
            {
                try
                {
                    list = PlaylistService.CreatePlaylist(FolderTree.SelectedItems[0].ToString());
                }
                catch (IOException) // name already taken, just fall back on default name
                {
                }
            }
            if (list == null)
            {
                list = PlaylistService.CreatePlaylist();
            }

            list.Songs.AddRange(from LibrarySongItem song in SongList.Items select new LocalSong(new FileInfo(song.GetPath())));
            list.Save();
            _playlists.Add(list);
            // TODO MC-207 flow to rename
        }

        /// <summary>
        /// Creates a new playlist and populates it based on the selected songs. If no
        /// songs are selected, the entire contents of the song list is used.
        /// 
        /// If there is only one selected folder, this is used as the playlist name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddSongsToNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Playlist list = null;
            if (FolderTree.SelectedItems.Count == 1)
            {
                try
                {
                    list = PlaylistService.CreatePlaylist(FolderTree.SelectedItems[0].ToString());
                }
                catch (IOException) // name already taken, just fall back on default name
                {
                }
            }
            if (list == null)
            {
                list = PlaylistService.CreatePlaylist();
            }

            var songs = SongList.SelectedItems.Count > 0 ? // No specific songs implies the whole thing
                SongList.SelectedItems :
                SongList.Items;

            list.Songs.AddRange(from LibrarySongItem song in songs select new LocalSong(new FileInfo(song.GetPath())));
            list.Save();
            _playlists.Add(list);
            // TODO MC-207 flow to rename
        }

        /// <summary>
        /// Adds the selected songs to the selected playlist. If there is no song selected, 
        /// the entire song list is added. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddSongsToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var list = ((MenuItem) sender).Header as Playlist;
            if (list == null)
            {
                return;
            }

            var songs = SongList.SelectedItems.Count > 0 ? // No specific songs implies the whole thing
                SongList.SelectedItems :
                SongList.Items;

            list.Songs.AddRange(from LibrarySongItem song in songs select new LocalSong(new FileInfo(song.GetPath())));
            list.Save();
        }

        /// <summary>
        /// Adds the selected folders to the selected playlist. If no folder is selected, 
        /// nothing is added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddFoldersToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItems.Count <= 0)
                return;
            var list = ((MenuItem)sender).Header as Playlist;
            if (list == null)
            {
                return;
            }

            list.Songs.AddRange(from LibrarySongItem song in SongList.Items select new LocalSong(new FileInfo(song.GetPath())));
            list.Save();
        }

        /// <summary>
        /// Saves the currently playing queue as a playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAsPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            if (NowPlaying.Inst.SongCount() < 1)
                return;

            var list = PlaylistService.CreatePlaylist();
            list.Songs.AddRange(NowPlaying.Inst.Playlist.Songs);
            _playlists.Add(list);
            list.Save();
            _nowPlayingDirty.Value = false;
            // TODO MC-207 flow to rename
        }

        #endregion
    }
}
