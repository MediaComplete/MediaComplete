using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib.Playlists;
using MSOE.MediaComplete.Lib.Songs;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Contains methods and properties related to playlists and playlist handling.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Bindable list of Playlists. Gets used in context menus and elsewhere.
        /// </summary>
        public ObservableCollection<Playlist> Playlists {
            get { return _playlists; }
        }
        private readonly ObservableCollection<Playlist> _playlists = new ObservableCollection<Playlist>(PlaylistService.GetAllPlaylists());

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

            var list = FolderTree.SelectedItems.Count == 1 ? 
                PlaylistService.CreatePlaylist(FolderTree.SelectedItems[0].ToString()) : 
                PlaylistService.CreatePlaylist();

            list.Songs.AddRange(from SongTreeViewItem song in SongTree.Items select new LocalSong(new FileInfo(song.GetPath())));
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
            var list = FolderTree.SelectedItems.Count == 1 ?
                PlaylistService.CreatePlaylist(FolderTree.SelectedItems[0].ToString()) :
                PlaylistService.CreatePlaylist();

            var songs = SongTree.SelectedItems.Count > 0 ? // No specific songs implies the whole thing
                SongTree.SelectedItems :
                SongTree.Items;

            list.Songs.AddRange(from SongTreeViewItem song in songs select new LocalSong(new FileInfo(song.GetPath())));
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

            var songs = SongTree.SelectedItems.Count > 0 ? // No specific songs implies the whole thing
                SongTree.SelectedItems :
                SongTree.Items;

            list.Songs.AddRange(from SongTreeViewItem song in songs select new LocalSong(new FileInfo(song.GetPath())));
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

            var list = (Playlist)((MenuItem)sender).Header;

            list.Songs.AddRange(from SongTreeViewItem song in SongTree.Items select new LocalSong(new FileInfo(song.GetPath())));
            list.Save();
        }
    }
}
