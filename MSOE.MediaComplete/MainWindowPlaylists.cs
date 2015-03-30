using System.Linq;
using System.Windows;
using MSOE.MediaComplete.Lib.Playlists;

namespace MSOE.MediaComplete
{
    /// <summary>
    /// Handles logic in the Playlists treeview tab.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Loads the playlists.
        /// </summary>
        private void InitPlaylistTree()
        {
            // TODO MC-204 support playlist folders
            PlaylistService.GetAllPlaylists().ToList().ForEach(p => PlaylistTree.Items.Add(p));
        }

        /// <summary>
        /// Creates a new playlist and shows it in the treeview.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_AddNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var p = PlaylistService.CreatePlaylist();
            p.Save();
            PlaylistTree.Items.Add(p);
            // TODO MC-207 flow into rename
        }
    }
}
